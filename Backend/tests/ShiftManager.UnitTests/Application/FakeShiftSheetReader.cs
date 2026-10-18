using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;

namespace ShiftManager.UnitTests.Application;

internal sealed class FakeShiftSheetReader(Result<IReadOnlyList<ShiftSheetRow>> result) : IShiftSheetReader
{
    public FakeShiftSheetReader(params ShiftSheetRow[] rows)
        : this(Result<IReadOnlyList<ShiftSheetRow>>.Success(rows))
    {
    }

    public Result<IReadOnlyList<ShiftSheetRow>> Read(Stream content) => result;
}
