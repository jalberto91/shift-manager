using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts;

public sealed record ShiftDto(Guid Id, Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime)
{
    public static ShiftDto From(Shift shift) =>
        new(shift.Id, shift.CollaboratorId, shift.Date, shift.StartTime, shift.EndTime);
}
