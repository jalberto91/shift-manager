using Mediator;
using ShiftManager.Domain.Common;

namespace ShiftManager.Application.Collaborators.DeactivateCollaborator;

public sealed record DeactivateCollaboratorCommand(Guid Id) : ICommand<Result>;
