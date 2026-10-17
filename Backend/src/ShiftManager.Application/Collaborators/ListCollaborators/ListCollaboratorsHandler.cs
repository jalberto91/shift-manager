using Mediator;
using ShiftManager.Application.Abstractions;

namespace ShiftManager.Application.Collaborators.ListCollaborators;

public sealed class ListCollaboratorsHandler(ICollaboratorRepository collaborators)
    : IQueryHandler<ListCollaboratorsQuery, IReadOnlyList<CollaboratorDto>>
{
    public async ValueTask<IReadOnlyList<CollaboratorDto>> Handle(
        ListCollaboratorsQuery query, CancellationToken cancellationToken)
    {
        var list = await collaborators.ListAsync(cancellationToken);

        return list.Select(CollaboratorDto.From).ToList();
    }
}
