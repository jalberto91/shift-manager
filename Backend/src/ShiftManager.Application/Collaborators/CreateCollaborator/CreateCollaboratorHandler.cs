using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Collaborators.CreateCollaborator;

public sealed class CreateCollaboratorHandler(ICollaboratorRepository collaborators, ICountryCatalog countries)
    : ICommandHandler<CreateCollaboratorCommand, Result<CollaboratorDto>>
{
    public async ValueTask<Result<CollaboratorDto>> Handle(
        CreateCollaboratorCommand command, CancellationToken cancellationToken)
    {
        var created = Collaborator.Create(command.Name, command.Email, command.CountryCode);
        if (created.IsFailure)
        {
            return Result<CollaboratorDto>.Failure(created.Errors);
        }

        var collaborator = created.Value;

        if (await countries.IsKnownAsync(collaborator.CountryCode, cancellationToken) == false)
        {
            return CollaboratorErrors.UnknownCountry;
        }

        if (await collaborators.EmailExistsAsync(collaborator.Email, excludingId: null, cancellationToken))
        {
            return CollaboratorErrors.EmailTaken;
        }

        collaborators.Add(collaborator);
        await collaborators.SaveChangesAsync(cancellationToken);

        return CollaboratorDto.From(collaborator);
    }
}
