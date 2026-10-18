using ShiftManager.Application.Abstractions;
using ShiftManager.Application.Shifts.ExportShifts;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class ExportShiftsHandlerTests
{
    private static readonly DateOnly Day = new(2026, 3, 10);

    private readonly FakeShiftRepository _shifts = new();
    private readonly FakeCollaboratorRepository _collaborators = new();
    private readonly FakeShiftSheetWriter _writer = new();

    private Task<Result<byte[]>> Export(DateOnly? from = null, DateOnly? to = null) =>
        new ExportShiftsHandler(_shifts, _collaborators, _writer)
            .Handle(new ExportShiftsQuery(from, to), CancellationToken.None).AsTask();

    [Fact]
    public async Task Writes_one_row_per_shift_with_the_collaborator_email()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "BR");
        _shifts.Seed(ana.Id, Day, "09:00", "13:00");
        _shifts.Seed(bruno.Id, Day.AddDays(1), "14:00", "18:30");

        var result = await Export();

        Assert.Equal(FakeShiftSheetWriter.Output, result.Value);
        Assert.Equal(
            [
                new ShiftExportRow("ana@example.com", Day, new(9, 0), new(13, 0)),
                new ShiftExportRow("bruno@example.com", Day.AddDays(1), new(14, 0), new(18, 30))
            ],
            _writer.Written);
    }

    [Fact]
    public async Task Keeps_only_the_shifts_inside_the_range_with_both_ends_included()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        _shifts.Seed(ana.Id, Day.AddDays(-1), "09:00", "13:00");
        _shifts.Seed(ana.Id, Day, "09:00", "13:00");
        _shifts.Seed(ana.Id, Day.AddDays(2), "09:00", "13:00");
        _shifts.Seed(ana.Id, Day.AddDays(3), "09:00", "13:00");

        await Export(from: Day, to: Day.AddDays(2));

        Assert.Equal([Day, Day.AddDays(2)], _writer.Written!.Select(row => row.Date));
    }

    [Fact]
    public async Task Exports_the_shifts_of_a_deactivated_collaborator()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        _shifts.Seed(ana.Id, Day, "09:00", "13:00");
        ana.Deactivate();

        await Export();

        Assert.Equal("ana@example.com", Assert.Single(_writer.Written!).Email);
    }

    [Fact]
    public async Task Writes_an_empty_sheet_when_the_range_has_no_shifts()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Export(from: Day.AddDays(5));

        Assert.True(result.IsSuccess);
        Assert.Empty(_writer.Written!);
    }

    [Fact]
    public async Task Rejects_a_from_date_later_than_the_to_date_without_writing()
    {
        var result = await Export(from: Day.AddDays(1), to: Day);

        Assert.Equal([ShiftErrors.DateRange], result.Errors);
        Assert.Null(_writer.Written);
    }
}
