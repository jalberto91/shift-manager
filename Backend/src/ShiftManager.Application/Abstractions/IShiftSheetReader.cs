using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Abstractions;

public interface IShiftSheetReader
{
    Result<IReadOnlyList<ShiftSheetRow>> Read(Stream content);
}
