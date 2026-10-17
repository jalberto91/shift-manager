using Mediator;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Collaborators.UpdateCollaborator;

public sealed record UpdateCollaboratorCommand(Guid Id, string? Name, string? Email, string? CountryCode)
    : ICommand<Result<CollaboratorDto>>;
