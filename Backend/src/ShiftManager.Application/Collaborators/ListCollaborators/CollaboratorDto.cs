using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Application.Collaborators.ListCollaborators;

public sealed record CollaboratorDto(Guid Id, string Name, string Email, string CountryCode)
{
    public static CollaboratorDto From(Collaborator collaborator) =>
        new(collaborator.Id, collaborator.Name, collaborator.Email, collaborator.CountryCode);
}
