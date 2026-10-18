namespace ShiftManager.Api.Contracts;

public sealed record ShiftRequest(Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
