using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.GetShift;

public sealed record GetShiftQuery(Guid Id) : IQuery<Result<ShiftDto>>;
