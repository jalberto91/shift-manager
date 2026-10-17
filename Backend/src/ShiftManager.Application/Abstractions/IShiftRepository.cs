using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Abstractions;

public interface IShiftRepository
{
    Task<IReadOnlyList<Shift>> ListByCollaboratorAndDateAsync(
        Guid collaboratorId, DateOnly date, CancellationToken cancellationToken);

    void Add(Shift shift);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
