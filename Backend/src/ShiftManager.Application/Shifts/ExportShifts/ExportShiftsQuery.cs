using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.ExportShifts;

public sealed record ExportShiftsQuery(DateOnly? From, DateOnly? To) : IQuery<Result<byte[]>>;
