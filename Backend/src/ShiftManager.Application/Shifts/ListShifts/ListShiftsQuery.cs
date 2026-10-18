using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.ListShifts;

public sealed record ListShiftsQuery(DateOnly? From, DateOnly? To) : IQuery<Result<IReadOnlyList<ShiftDto>>>;
