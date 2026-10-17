using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Application.Abstractions;

public interface ICollaboratorRepository
{
    Task<IReadOnlyList<Collaborator>> ListAsync(CancellationToken cancellationToken);

    Task<Collaborator?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, Guid? excludingId, CancellationToken cancellationToken);

    void Add(Collaborator collaborator);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
