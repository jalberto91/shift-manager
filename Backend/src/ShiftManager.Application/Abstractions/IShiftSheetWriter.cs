namespace ShiftManager.Application.Abstractions;

public interface IShiftSheetWriter
{
    byte[] Write(IReadOnlyList<ShiftExportRow> rows);
}
