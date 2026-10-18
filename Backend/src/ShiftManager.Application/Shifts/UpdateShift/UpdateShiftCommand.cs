using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.UpdateShift;

public sealed record UpdateShiftCommand(Guid Id, Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime)
    : ICommand<Result<ShiftDto>>;
