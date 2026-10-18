using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Abstractions;

public interface IShiftRepository
{
    Task<IReadOnlyList<Shift>> ListByCollaboratorAndDateAsync(
        Guid collaboratorId, DateOnly date, CancellationToken cancellationToken);

    Task<IReadOnlyList<Shift>> ListAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken);

    Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Shift shift);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
