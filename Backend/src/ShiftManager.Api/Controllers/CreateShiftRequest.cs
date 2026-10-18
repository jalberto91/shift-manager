namespace ShiftManager.Api.Controllers;

public sealed record CreateShiftRequest(Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
