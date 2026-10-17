using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Collaborators.UpdateCollaborator;

public sealed class UpdateCollaboratorHandler(ICollaboratorRepository collaborators, ICountryCatalog countries)
    : ICommandHandler<UpdateCollaboratorCommand, Result<CollaboratorDto>>
{
    public async ValueTask<Result<CollaboratorDto>> Handle(
        UpdateCollaboratorCommand command, CancellationToken cancellationToken)
    {
        var collaborator = await collaborators.GetByIdAsync(command.Id, cancellationToken);
        if (collaborator is not { IsActive: true })
        {
            return CollaboratorErrors.NotFound;
        }

        var updated = collaborator.Update(command.Name, command.Email, command.CountryCode);
        if (updated.IsFailure)
        {
            return Result<CollaboratorDto>.Failure(updated.Errors);
        }

        if (await countries.IsKnownAsync(collaborator.CountryCode, cancellationToken) == false)
        {
            return CollaboratorErrors.UnknownCountry;
        }

        if (await collaborators.EmailExistsAsync(collaborator.Email, excludingId: collaborator.Id, cancellationToken))
        {
            return CollaboratorErrors.EmailTaken;
        }

        await collaborators.SaveChangesAsync(cancellationToken);

        return CollaboratorDto.From(collaborator);
    }
}
