using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Collaborators.DeactivateCollaborator;

public sealed class DeactivateCollaboratorHandler(ICollaboratorRepository collaborators)
    : ICommandHandler<DeactivateCollaboratorCommand, Result>
{
    public async ValueTask<Result> Handle(DeactivateCollaboratorCommand command, CancellationToken cancellationToken)
    {
        var collaborator = await collaborators.GetByIdAsync(command.Id, cancellationToken);
        if (collaborator is null)
        {
            return Result.Failure(CollaboratorErrors.NotFound);
        }

        collaborator.Deactivate();
        await collaborators.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
