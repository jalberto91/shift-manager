using ShiftManager.Application.Abstractions;
using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.ImportShifts;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class ImportShiftsHandlerTests
{
    private static readonly DateOnly Day = new(2026, 3, 10);

    private readonly FakeCollaboratorRepository _collaborators = new();
    private readonly FakeShiftRepository _shifts = new();

    private static ShiftSheetRow Row(int number, string email, string start, string end, DateOnly? date = null) =>
        new(number, email, date ?? Day, TimeOnly.Parse(start), TimeOnly.Parse(end));

    private Task<Result<ImportReportDto>> Import(params ShiftSheetRow[] rows) =>
        Import(new FakeShiftSheetReader(rows));

    private Task<Result<ImportReportDto>> Import(IShiftSheetReader reader) =>
        new ImportShiftsHandler(reader, _collaborators, new ShiftPlacementChecker(_collaborators, _shifts), _shifts)
            .Handle(new ImportShiftsCommand(Stream.Null), CancellationToken.None).AsTask();

    private static void AssertSingleFailure(ImportReportDto report, int row, params Error[] errors)
    {
        var failure = Assert.Single(report.FailedRows);
        Assert.Equal(row, failure.Row);
        Assert.Equal(errors.Select(error => error.Message), failure.Reasons);
    }

    [Fact]
    public async Task Imports_valid_rows_and_saves_once()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "BR");

        var result = await Import(
            Row(2, "ana@example.com", "09:00", "13:00"),
            Row(3, "bruno@example.com", "09:00", "13:00"));

        Assert.True(result.IsSuccess);
        Assert.Equal([2, 3], result.Value.ImportedRows);
        Assert.Empty(result.Value.FailedRows);
        Assert.Equal([ana.Id, bruno.Id], _shifts.Stored.Select(s => s.CollaboratorId));
        Assert.Equal(1, _shifts.SaveCount);
    }

    [Fact]
    public async Task Matches_the_email_ignoring_case_and_spaces()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Import(Row(2, "  Ana@Example.COM ", "09:00", "13:00"));

        Assert.Equal([2], result.Value.ImportedRows);
        Assert.Equal(ana.Id, _shifts.Stored.Single().CollaboratorId);
    }

    [Fact]
    public async Task Imports_the_valid_rows_and_reports_the_failed_ones()
    {
        _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Import(
            Row(2, "ana@example.com", "09:00", "13:00"),
            Row(3, "ana@example.com", "18:00", "14:00"),
            Row(4, "ana@example.com", "14:00", "18:00"));

        Assert.Equal([2, 4], result.Value.ImportedRows);
        AssertSingleFailure(result.Value, 3, ShiftErrors.InvalidTimeRange);
        Assert.Equal(2, _shifts.Stored.Count);
    }

    [Fact]
    public async Task Reports_every_reason_of_a_failed_row()
    {
        _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Import(Row(2, "ana@example.com", "18:00", "14:00", date: default(DateOnly)));

        var failure = Assert.Single(result.Value.FailedRows);
        Assert.Equal([ShiftErrors.InvalidDate.Message, ShiftErrors.InvalidTimeRange.Message], failure.Reasons);
    }

    [Fact]
    public async Task Fails_a_row_with_an_unknown_email()
    {
        var result = await Import(Row(2, "nobody@example.com", "09:00", "13:00"));

        Assert.Empty(result.Value.ImportedRows);
        AssertSingleFailure(result.Value, 2, ShiftErrors.CollaboratorUnavailable);
        Assert.Equal(0, _shifts.SaveCount);
    }

    [Fact]
    public async Task Fails_a_row_of_an_inactive_collaborator()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        ana.Deactivate();

        var result = await Import(Row(2, "ana@example.com", "09:00", "13:00"));

        AssertSingleFailure(result.Value, 2, ShiftErrors.CollaboratorUnavailable);
        Assert.Empty(_shifts.Stored);
    }

    [Fact]
    public async Task Reports_the_cell_error_of_an_unreadable_row()
    {
        var cellError = ShiftErrors.InvalidCell("Fecha");

        var result = await Import(ShiftSheetRow.Unreadable(2, cellError));

        AssertSingleFailure(result.Value, 2, cellError);
    }

    [Fact]
    public async Task Fails_the_second_of_two_overlapping_rows_of_the_file()
    {
        _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Import(
            Row(2, "ana@example.com", "09:00", "13:00"),
            Row(3, "ana@example.com", "12:00", "16:00"));

        Assert.Equal([2], result.Value.ImportedRows);
        AssertSingleFailure(result.Value, 3, ShiftErrors.OverlapsEarlierRow);
        Assert.Single(_shifts.Stored);
    }

    [Fact]
    public async Task Fails_a_row_that_overlaps_a_stored_shift()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var existing = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Import(Row(2, "ana@example.com", "12:00", "16:00"));

        AssertSingleFailure(result.Value, 2, ShiftErrors.Overlap(existing.Id));
        Assert.Single(_shifts.Stored);
    }

    [Fact]
    public async Task Accepts_rows_that_only_touch_the_boundary_or_belong_to_other_collaborators()
    {
        _collaborators.Seed("Ana", "ana@example.com", "ES");
        _collaborators.Seed("Bruno", "bruno@example.com", "BR");

        var result = await Import(
            Row(2, "ana@example.com", "09:00", "13:00"),
            Row(3, "ana@example.com", "13:00", "17:00"),
            Row(4, "bruno@example.com", "09:00", "13:00"));

        Assert.Equal([2, 3, 4], result.Value.ImportedRows);
        Assert.Empty(result.Value.FailedRows);
    }

    [Fact]
    public async Task Returns_the_reader_error_without_saving()
    {
        var fileError = Error.Validation("shift.import.columns", "Missing columns.");
        var reader = new FakeShiftSheetReader(Result<IReadOnlyList<ShiftSheetRow>>.Failure(fileError));

        var result = await Import(reader);

        Assert.Equal([fileError], result.Errors);
        Assert.Equal(0, _shifts.SaveCount);
    }

    [Fact]
    public async Task Reports_an_empty_import_without_saving()
    {
        var result = await Import();

        Assert.Empty(result.Value.ImportedRows);
        Assert.Empty(result.Value.FailedRows);
        Assert.Equal(0, _shifts.SaveCount);
    }
}
