using Mediator;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Collaborators.CreateCollaborator;

public sealed record CreateCollaboratorCommand(string? Name, string? Email, string? CountryCode)
    : ICommand<Result<CollaboratorDto>>;
