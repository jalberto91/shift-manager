using ShiftManager.Application.Abstractions;

namespace ShiftManager.Application.Shifts.ListShifts;

public sealed record HolidayInfo(HolidayStatus Status, string? Name, bool? IsGlobal)
{
    public static readonly HolidayInfo None = new(HolidayStatus.None, null, null);

    public static readonly HolidayInfo Unknown = new(HolidayStatus.Unknown, null, null);

    public static HolidayInfo From(PublicHoliday holiday) => new(HolidayStatus.Holiday, holiday.Name, holiday.IsGlobal);
}
