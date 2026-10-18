namespace ShiftManager.Application.Abstractions;

public sealed record PublicHoliday(DateOnly Date, string Name, bool IsGlobal);
