using ClosedXML.Excel;
using ShiftManager.Application.Abstractions;
using ShiftManager.Infrastructure.Spreadsheets;

namespace ShiftManager.UnitTests.Infrastructure;

public class ClosedXmlShiftSheetWriterTests
{
    private static readonly ShiftExportRow[] Rows =
    [
        new("ana@example.com", new(2026, 3, 10), new(9, 0), new(13, 30)),
        new("bruno@example.com", new(2026, 3, 11), new(8, 15), new(12, 0)),
    ];

    private static XLWorkbook Open(byte[] content) => new(new MemoryStream(content));

    [Fact]
    public void Written_rows_are_read_back_in_order()
    {
        var content = new ClosedXmlShiftSheetWriter().Write(Rows);

        var result = new ClosedXmlShiftSheetReader().Read(new MemoryStream(content));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [
                new ShiftSheetRow(2, "ana@example.com", new(2026, 3, 10), new(9, 0), new(13, 30)),
                new ShiftSheetRow(3, "bruno@example.com", new(2026, 3, 11), new(8, 15), new(12, 0)),
            ],
            result.Value);
    }

    [Fact]
    public void Empty_range_writes_only_the_header()
    {
        var content = new ClosedXmlShiftSheetWriter().Write([]);

        var result = new ClosedXmlShiftSheetReader().Read(new MemoryStream(content));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void Date_and_times_are_real_cells_with_the_expected_format()
    {
        using var workbook = Open(new ClosedXmlShiftSheetWriter().Write(Rows));
        var sheet = workbook.Worksheet("Shifts");

        Assert.Equal(XLDataType.DateTime, sheet.Cell(2, 2).DataType);
        Assert.Equal("yyyy-mm-dd", sheet.Cell(2, 2).Style.DateFormat.Format);
        Assert.Equal(XLDataType.TimeSpan, sheet.Cell(2, 3).DataType);
        Assert.Equal("hh:mm", sheet.Cell(2, 3).Style.DateFormat.Format);
        Assert.Equal(XLDataType.TimeSpan, sheet.Cell(2, 4).DataType);
        Assert.Equal("hh:mm", sheet.Cell(2, 4).Style.DateFormat.Format);
    }

    [Fact]
    public void Header_is_bold_and_matches_the_import_columns()
    {
        using var workbook = Open(new ClosedXmlShiftSheetWriter().Write(Rows));
        var header = workbook.Worksheet("Shifts").Row(1);

        Assert.Equal(["Email", "Fecha", "HoraInicio", "HoraFin"], header.Cells(1, 4).Select(cell => cell.GetString()));
        Assert.True(header.Style.Font.Bold);
    }
}
