namespace ShiftManager.Application.Shifts.ImportShifts;

public sealed record ImportReportDto(IReadOnlyList<int> ImportedRows, IReadOnlyList<ImportFailureDto> FailedRows);
