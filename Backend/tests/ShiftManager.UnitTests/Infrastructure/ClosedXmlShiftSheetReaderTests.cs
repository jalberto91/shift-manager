using ClosedXML.Excel;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Shifts;
using ShiftManager.Infrastructure.Spreadsheets;

namespace ShiftManager.UnitTests.Infrastructure;

public class ClosedXmlShiftSheetReaderTests
{
    private static readonly string[] Header = ["Email", "Fecha", "HoraInicio", "HoraFin"];

    private static MemoryStream Workbook(string[] header, params object[][] rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet();

        for (var column = 0; column < header.Length; column++)
        {
            sheet.Cell(1, column + 1).Value = header[column];
        }

        for (var row = 0; row < rows.Length; row++)
        {
            for (var column = 0; column < rows[row].Length; column++)
            {
                sheet.Cell(row + 2, column + 1).Value = XLCellValue.FromObject(rows[row][column]);
            }
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static ShiftSheetRow Single(Stream content)
    {
        var result = new ClosedXmlShiftSheetReader().Read(content);

        Assert.True(result.IsSuccess);
        return Assert.Single(result.Value);
    }

    [Fact]
    public void Reads_date_and_time_cells()
    {
        var row = Single(Workbook(
            Header, ["ana@example.com", new DateTime(2026, 3, 10), new TimeSpan(9, 0, 0), new TimeSpan(13, 30, 0)]));

        Assert.Equal(new ShiftSheetRow(2, "ana@example.com", new(2026, 3, 10), new(9, 0), new(13, 30)), row);
    }

    [Fact]
    public void Reads_date_and_time_written_as_text()
    {
        var content = Workbook(
            Header,
            ["ana@example.com", "2026-03-10", "09:00", "13:30"],
            ["bruno@example.com", "11/03/2026", "08:15", "12:00"]);

        var result = new ClosedXmlShiftSheetReader().Read(content);

        Assert.Equal(
            [
                new ShiftSheetRow(2, "ana@example.com", new(2026, 3, 10), new(9, 0), new(13, 30)),
                new ShiftSheetRow(3, "bruno@example.com", new(2026, 3, 11), new(8, 15), new(12, 0))
            ],
            result.Value);
    }

    [Fact]
    public void Finds_the_columns_by_name_in_any_order_and_case()
    {
        var row = Single(Workbook(
            ["notes", "HORAFIN", "email", "horainicio", "FECHA"],
            ["ignored", "13:00", "ana@example.com", "09:00", "2026-03-10"]));

        Assert.Equal(new ShiftSheetRow(2, "ana@example.com", new(2026, 3, 10), new(9, 0), new(13, 0)), row);
    }

    [Fact]
    public void Keeps_the_excel_row_number_and_skips_empty_rows()
    {
        var content = Workbook(
            Header,
            ["ana@example.com", "2026-03-10", "09:00", "13:00"],
            [],
            ["ana@example.com", "2026-03-11", "09:00", "13:00"]);

        var result = new ClosedXmlShiftSheetReader().Read(content);

        Assert.Equal([2, 4], result.Value.Select(row => row.RowNumber));
    }

    [Theory]
    [InlineData("", "2026-03-10", "09:00", "13:00", "Email")]
    [InlineData("ana@example.com", "10 March", "09:00", "13:00", "Fecha")]
    [InlineData("ana@example.com", "2026-03-10", "nine", "13:00", "HoraInicio")]
    [InlineData("ana@example.com", "2026-03-10", "09:00", "", "HoraFin")]
    public void Marks_an_unreadable_row_with_the_offending_column(
        string email, string date, string start, string end, string column)
    {
        var row = Single(Workbook(Header, [email, date, start, end, "x"]));

        Assert.Equal(ShiftErrors.InvalidCell(column), row.CellError);
        Assert.Equal(2, row.RowNumber);
    }

    [Fact]
    public void Marks_a_duration_of_a_day_or_more_as_an_unreadable_time()
    {
        var row = Single(Workbook(
            Header, ["ana@example.com", new DateTime(2026, 3, 10), new TimeSpan(25, 0, 0), new TimeSpan(13, 0, 0)]));

        Assert.Equal(ShiftErrors.InvalidCell("HoraInicio"), row.CellError);
    }

    [Fact]
    public void Fails_when_required_columns_are_missing()
    {
        var result = new ClosedXmlShiftSheetReader().Read(Workbook(["Email", "Fecha"]));

        Assert.Equal([ShiftErrors.MissingColumns(["HoraInicio", "HoraFin"])], result.Errors);
    }

    [Fact]
    public void Returns_no_rows_for_a_file_with_only_the_header()
    {
        var result = new ClosedXmlShiftSheetReader().Read(Workbook(Header));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void Fails_when_the_file_is_not_a_workbook()
    {
        var result = new ClosedXmlShiftSheetReader().Read(new MemoryStream("not an xlsx"u8.ToArray()));

        Assert.Equal([ShiftErrors.InvalidFile], result.Errors);
    }
}
