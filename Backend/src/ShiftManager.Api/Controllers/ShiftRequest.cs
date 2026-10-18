namespace ShiftManager.Api.Controllers;

public sealed record ShiftRequest(Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
