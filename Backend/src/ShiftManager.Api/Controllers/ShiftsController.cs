using Mediator;
using Microsoft.AspNetCore.Mvc;
using ShiftManager.Api.Contracts;
using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.CreateShift;
using ShiftManager.Application.Shifts.DeleteShift;
using ShiftManager.Application.Shifts.GetShift;
using ShiftManager.Application.Shifts.ImportShifts;
using ShiftManager.Application.Shifts.ListShifts;
using ShiftManager.Application.Shifts.UpdateShift;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Api.Controllers;

[Route("api/shifts")]
public sealed class ShiftsController(IMediator mediator) : ApiControllerBase
{
    private const long MaxImportBytes = 5 * 1024 * 1024;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ShiftListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListShiftsQuery(from, to), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetShiftQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result);
    }

    [HttpPost]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateShiftCommand(request.CollaboratorId, request.Date, request.StartTime, request.EndTime),
            cancellationToken);

        return result.IsSuccess
            ? Created($"/api/shifts/{result.Value.Id}", result.Value)
            : ToProblem(result);
    }

    [HttpPost("import")]
    [ProducesResponseType<ImportReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return ToProblem(Result.Failure(ShiftErrors.InvalidFile));
        }

        if (file.Length > MaxImportBytes)
        {
            return ToProblem(Result.Failure(ShiftErrors.FileTooLarge));
        }

        await using var content = file.OpenReadStream();
        var result = await mediator.Send(new ImportShiftsCommand(content), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ShiftDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, ShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateShiftCommand(id, request.CollaboratorId, request.Date, request.StartTime, request.EndTime),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteShiftCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : ToProblem(result);
    }
}
