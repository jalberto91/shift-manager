using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Abstractions;

public sealed record ShiftSheetRow(
    int RowNumber, string Email, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, Error? CellError = null)
{
    public static ShiftSheetRow Unreadable(int rowNumber, Error cellError) =>
        new(rowNumber, string.Empty, default, default, default, cellError);
}
