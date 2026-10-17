using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.CreateShift;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class CreateShiftHandlerTests
{
    private static readonly DateOnly Day = new(2026, 3, 10);

    private readonly FakeCollaboratorRepository _collaborators = new();
    private readonly FakeShiftRepository _shifts = new();

    private Task<Result<ShiftDto>> Create(Guid collaboratorId, string start, string end, DateOnly? date = null) =>
        new CreateShiftHandler(_collaborators, _shifts)
            .Handle(
                new CreateShiftCommand(collaboratorId, date ?? Day, TimeOnly.Parse(start), TimeOnly.Parse(end)),
                CancellationToken.None).AsTask();

    [Fact]
    public async Task Creates_and_saves_the_shift()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Create(ana.Id, "09:00", "13:00");

        Assert.True(result.IsSuccess);
        Assert.Equal(ana.Id, result.Value.CollaboratorId);
        Assert.Single(_shifts.Stored);
        Assert.Equal(1, _shifts.SaveCount);
    }

    [Fact]
    public async Task Rejects_an_invalid_range_without_saving()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Create(ana.Id, "13:00", "09:00");

        Assert.Equal([ShiftErrors.InvalidTimeRange], result.Errors);
        Assert.Empty(_shifts.Stored);
    }

    [Fact]
    public async Task Rejects_an_unknown_collaborator()
    {
        var result = await Create(Guid.NewGuid(), "09:00", "13:00");

        Assert.Equal([ShiftErrors.CollaboratorUnavailable], result.Errors);
        Assert.Empty(_shifts.Stored);
    }

    [Fact]
    public async Task Rejects_an_inactive_collaborator()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        ana.Deactivate();

        var result = await Create(ana.Id, "09:00", "13:00");

        Assert.Equal([ShiftErrors.CollaboratorUnavailable], result.Errors);
        Assert.Empty(_shifts.Stored);
    }

    [Fact]
    public async Task Returns_a_conflict_with_the_overlapping_shift_id()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var existing = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Create(ana.Id, "12:00", "16:00");

        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.Overlap(existing.Id), result.Error);
        Assert.Equal(existing.Id, result.Error!.Extensions!["conflictingShiftId"]);
        Assert.Single(_shifts.Stored);
        Assert.Equal(0, _shifts.SaveCount);
    }

    [Fact]
    public async Task Reports_the_shift_that_actually_overlaps_among_several()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        _shifts.Seed(ana.Id, Day, "06:00", "08:00");
        var overlapping = _shifts.Seed(ana.Id, Day, "14:00", "18:00");

        var result = await Create(ana.Id, "17:00", "20:00");

        Assert.Equal(overlapping.Id, result.Error!.Extensions!["conflictingShiftId"]);
    }

    [Fact]
    public async Task Accepts_a_shift_that_only_touches_the_boundary()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Create(ana.Id, "13:00", "17:00");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, _shifts.Stored.Count);
    }

    [Fact]
    public async Task Ignores_shifts_of_other_collaborators_and_other_days()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "BR");
        _shifts.Seed(bruno.Id, Day, "09:00", "13:00");
        _shifts.Seed(ana.Id, Day.AddDays(1), "09:00", "13:00");

        var result = await Create(ana.Id, "09:00", "13:00");

        Assert.True(result.IsSuccess);
    }
}
