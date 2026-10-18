using Mediator;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.CreateShift;

namespace ShiftManager.Api.Controllers;

[Route("api/shifts")]
public sealed class ShiftsController(IMediator mediator) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateShiftCommand(request.CollaboratorId, request.Date, request.StartTime, request.EndTime),
            cancellationToken);

        return result.IsSuccess
            ? Created($"/api/shifts/{result.Value.Id}", result.Value)
            : ToProblem(result);
    }
}
