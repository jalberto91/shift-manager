using ShiftManager.Application.Abstractions;

namespace ShiftManager.UnitTests.Application;

internal sealed class FakeShiftSheetWriter : IShiftSheetWriter
{
    public static readonly byte[] Output = [1, 2, 3];

    public IReadOnlyList<ShiftExportRow>? Written { get; private set; }

    public byte[] Write(IReadOnlyList<ShiftExportRow> rows)
    {
        Written = rows;
        return Output;
    }
}
