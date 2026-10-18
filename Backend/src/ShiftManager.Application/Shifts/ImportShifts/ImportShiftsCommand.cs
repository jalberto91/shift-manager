using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.ImportShifts;

public sealed record ImportShiftsCommand(Stream Content) : ICommand<Result<ImportReportDto>>;
