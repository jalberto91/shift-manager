using ClosedXML.Excel;
using ShiftManager.Application.Abstractions;

namespace ShiftManager.Infrastructure.Spreadsheets;

public sealed class ClosedXmlShiftSheetWriter : IShiftSheetWriter
{
    private static readonly string[] Header = ["Email", "Fecha", "HoraInicio", "HoraFin"];

    public byte[] Write(IReadOnlyList<ShiftExportRow> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Shifts");

        for (var column = 0; column < Header.Length; column++)
        {
            sheet.Cell(1, column + 1).Value = Header[column];
        }

        sheet.Row(1).Style.Font.Bold = true;

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowNumber = index + 2;

            sheet.Cell(rowNumber, 1).Value = row.Email;
            sheet.Cell(rowNumber, 2).Value = row.Date.ToDateTime(TimeOnly.MinValue);
            sheet.Cell(rowNumber, 3).Value = row.StartTime.ToTimeSpan();
            sheet.Cell(rowNumber, 4).Value = row.EndTime.ToTimeSpan();
        }

        sheet.Column(2).Style.DateFormat.Format = "yyyy-mm-dd";
        sheet.Column(3).Style.DateFormat.Format = "hh:mm";
        sheet.Column(4).Style.DateFormat.Format = "hh:mm";
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
