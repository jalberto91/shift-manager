using Mediator;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Application.Collaborators.ListCollaborators;

namespace ShiftManager.Api.Controllers;

[Route("api/collaborators")]
public sealed class CollaboratorsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CollaboratorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ListCollaboratorsQuery(), cancellationToken));
}
