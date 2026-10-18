using System.Globalization;
using ClosedXML.Excel;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Infrastructure.Spreadsheets;

public sealed class ClosedXmlShiftSheetReader : IShiftSheetReader
{
    private const string EmailColumn = "Email";
    private const string DateColumn = "Fecha";
    private const string StartColumn = "HoraInicio";
    private const string EndColumn = "HoraFin";

    private static readonly string[] RequiredColumns = [EmailColumn, DateColumn, StartColumn, EndColumn];
    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy"];

    public Result<IReadOnlyList<ShiftSheetRow>> Read(Stream content)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(content);
        }
        catch (Exception)
        {
            // The parser throws a different exception type for each way a file can be malformed.
            return ShiftErrors.InvalidFile;
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.First();
            var header = sheet.FirstRowUsed();
            if (header is null)
            {
                return ShiftErrors.MissingColumns(RequiredColumns);
            }

            var columns = header.CellsUsed()
                .GroupBy(cell => cell.GetString().Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
            var missing = RequiredColumns.Where(column => !columns.ContainsKey(column)).ToList();
            if (missing.Count > 0)
            {
                return ShiftErrors.MissingColumns(missing);
            }

            var rows = sheet.RowsUsed()
                .Where(row => row.RowNumber() > header.RowNumber())
                .Select(row => ReadRow(row, columns))
                .ToList();

            return Result<IReadOnlyList<ShiftSheetRow>>.Success(rows);
        }
    }

    private static ShiftSheetRow ReadRow(IXLRow row, IReadOnlyDictionary<string, int> columns)
    {
        var rowNumber = row.RowNumber();
        var email = row.Cell(columns[EmailColumn]).GetString().Trim();

        if (email.Length == 0)
        {
            return ShiftSheetRow.Unreadable(rowNumber, ShiftErrors.InvalidCell(EmailColumn));
        }

        if (!TryReadDate(row.Cell(columns[DateColumn]), out var date))
        {
            return ShiftSheetRow.Unreadable(rowNumber, ShiftErrors.InvalidCell(DateColumn));
        }

        if (!TryReadTime(row.Cell(columns[StartColumn]), out var start))
        {
            return ShiftSheetRow.Unreadable(rowNumber, ShiftErrors.InvalidCell(StartColumn));
        }

        if (!TryReadTime(row.Cell(columns[EndColumn]), out var end))
        {
            return ShiftSheetRow.Unreadable(rowNumber, ShiftErrors.InvalidCell(EndColumn));
        }

        return new ShiftSheetRow(rowNumber, email, date, start, end);
    }

    private static bool TryReadDate(IXLCell cell, out DateOnly date)
    {
        if (cell.DataType == XLDataType.DateTime)
        {
            date = DateOnly.FromDateTime(cell.GetDateTime());
            return true;
        }

        return DateOnly.TryParseExact(
            cell.GetString().Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static bool TryReadTime(IXLCell cell, out TimeOnly time)
    {
        switch (cell.DataType)
        {
            case XLDataType.DateTime:
                time = TimeOnly.FromDateTime(cell.GetDateTime());
                return true;
            case XLDataType.TimeSpan when cell.GetTimeSpan() is { Ticks: >= 0 } span && span < TimeSpan.FromDays(1):
                time = TimeOnly.FromTimeSpan(span);
                return true;
            case XLDataType.TimeSpan:
                time = default;
                return false;
            default:
                return TimeOnly.TryParseExact(
                    cell.GetString().Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
        }
    }
}
