using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Shifts.ImportShifts;

public sealed record ImportFailureDto(int Row, IReadOnlyList<string> Reasons)
{
    public static ImportFailureDto From(int row, IEnumerable<Error> errors) =>
        new(row, errors.Select(error => error.Message).ToList());
}
