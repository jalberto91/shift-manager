using Mediator;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Api.Contracts;
using ShiftManager.Application.Collaborators.CreateCollaborator;
using ShiftManager.Application.Collaborators.DeactivateCollaborator;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Application.Collaborators.UpdateCollaborator;

namespace ShiftManager.Api.Controllers;

[Route("api/collaborators")]
public sealed class CollaboratorsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CollaboratorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ListCollaboratorsQuery(), cancellationToken));

    [HttpPost]
    [ProducesResponseType<CollaboratorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CollaboratorRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateCollaboratorCommand(request.Name, request.Email, request.CountryCode), cancellationToken);

        return result.IsSuccess
            ? Created($"/api/collaborators/{result.Value.Id}", result.Value)
            : ToProblem(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CollaboratorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, CollaboratorRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateCollaboratorCommand(id, request.Name, request.Email, request.CountryCode), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateCollaboratorCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : ToProblem(result);
    }
}
