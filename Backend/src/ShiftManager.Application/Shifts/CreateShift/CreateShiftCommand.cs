using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.CreateShift;

public sealed record CreateShiftCommand(Guid CollaboratorId, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime)
    : ICommand<Result<ShiftDto>>;
