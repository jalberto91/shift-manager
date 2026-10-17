namespace ShiftManager.Application.Collaborators.ListCollaborators;

public sealed record CollaboratorDto(Guid Id, string Name, string Email, string CountryCode);
