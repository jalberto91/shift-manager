using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.DeleteShift;

public sealed record DeleteShiftCommand(Guid Id) : ICommand<Result>;
