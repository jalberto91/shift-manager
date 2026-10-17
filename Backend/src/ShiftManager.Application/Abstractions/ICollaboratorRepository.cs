using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Application.Abstractions;

public interface ICollaboratorRepository
{
    Task<IReadOnlyList<Collaborator>> ListAsync(CancellationToken cancellationToken);
}
