namespace ShiftManager.Application.Abstractions;

public sealed record ShiftExportRow(string Email, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
