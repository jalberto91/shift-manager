using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.ListShifts;

public sealed record ShiftListItemDto(
    Guid Id, Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, HolidayInfo Holiday)
{
    public static ShiftListItemDto From(Shift shift, HolidayInfo holiday) =>
        new(shift.Id, shift.CollaboratorId, shift.Date, shift.StartTime, shift.EndTime, holiday);
}
