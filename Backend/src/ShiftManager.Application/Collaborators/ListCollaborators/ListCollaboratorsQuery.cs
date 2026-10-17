using Mediator;

namespace ShiftManager.Application.Collaborators.ListCollaborators;

public sealed record ListCollaboratorsQuery : IQuery<IReadOnlyList<CollaboratorDto>>;
