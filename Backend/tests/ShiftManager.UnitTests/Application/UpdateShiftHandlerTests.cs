using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.UpdateShift;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class UpdateShiftHandlerTests
{
    private static readonly DateOnly Day = new(2026, 3, 10);

    private readonly FakeCollaboratorRepository _collaborators = new();
    private readonly FakeShiftRepository _shifts = new();

    private Task<Result<ShiftDto>> Update(Guid id, Guid collaboratorId, string start, string end, DateOnly? date = null) =>
        new UpdateShiftHandler(_collaborators, _shifts)
            .Handle(
                new UpdateShiftCommand(id, collaboratorId, date ?? Day, TimeOnly.Parse(start), TimeOnly.Parse(end)),
                CancellationToken.None).AsTask();

    [Fact]
    public async Task Updates_and_saves_the_shift()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Update(shift.Id, ana.Id, "10:00", "14:00");

        Assert.True(result.IsSuccess);
        Assert.Equal(new TimeOnly(10, 0), shift.StartTime);
        Assert.Equal(1, _shifts.SaveCount);
    }

    [Fact]
    public async Task Returns_not_found_for_an_unknown_shift()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");

        var result = await Update(Guid.NewGuid(), ana.Id, "09:00", "13:00");

        Assert.Equal([ShiftErrors.NotFound], result.Errors);
    }

    [Fact]
    public async Task Rejects_an_invalid_range_without_saving()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Update(shift.Id, ana.Id, "13:00", "09:00");

        Assert.Equal([ShiftErrors.InvalidTimeRange], result.Errors);
        Assert.Equal(0, _shifts.SaveCount);
    }

    [Fact]
    public async Task Rejects_an_unknown_or_inactive_target_collaborator()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "BR");
        bruno.Deactivate();
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var unknown = await Update(shift.Id, Guid.NewGuid(), "09:00", "13:00");
        var inactive = await Update(shift.Id, bruno.Id, "09:00", "13:00");

        Assert.Equal([ShiftErrors.CollaboratorUnavailable], unknown.Errors);
        Assert.Equal([ShiftErrors.CollaboratorUnavailable], inactive.Errors);
        Assert.Equal(0, _shifts.SaveCount);
    }

    [Fact]
    public async Task Accepts_the_same_values_without_clashing_with_itself()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Update(shift.Id, ana.Id, "09:00", "13:00");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Accepts_widening_the_shift_inside_its_own_slot()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");

        var result = await Update(shift.Id, ana.Id, "08:00", "15:00");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Returns_a_conflict_when_moving_onto_a_busy_day()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");
        var other = _shifts.Seed(ana.Id, Day.AddDays(1), "09:00", "13:00");

        var result = await Update(shift.Id, ana.Id, "09:00", "13:00", Day.AddDays(1));

        Assert.Equal(ShiftErrors.Overlap(other.Id), result.Error);
        Assert.Equal(0, _shifts.SaveCount);
    }

    [Fact]
    public async Task Moves_the_shift_to_another_collaborator_when_it_does_not_clash()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "BR");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");
        _shifts.Seed(bruno.Id, Day, "13:00", "17:00");

        var result = await Update(shift.Id, bruno.Id, "09:00", "13:00");

        Assert.True(result.IsSuccess);
        Assert.Equal(bruno.Id, shift.CollaboratorId);
    }

    [Fact]
    public async Task Returns_a_conflict_when_the_new_collaborator_has_a_clashing_shift()
    {
        var ana = _collaborators.Seed("Ana", "ana@example.com", "ES");
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "BR");
        var shift = _shifts.Seed(ana.Id, Day, "09:00", "13:00");
        var other = _shifts.Seed(bruno.Id, Day, "12:00", "16:00");

        var result = await Update(shift.Id, bruno.Id, "09:00", "13:00");

        Assert.Equal(ShiftErrors.Overlap(other.Id), result.Error);
    }
}
