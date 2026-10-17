using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using ShiftManager.Domain.Common;

namespace ShiftManager.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToProblem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result has no problem to report.");
        }

        // El primer error decide el status; todos aparecen en `errors`.
        var first = result.Errors[0];
        var status = StatusFor(first.Type);

        var modelState = new ModelStateDictionary();
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.Code, error.Message);
        }

        var problem = ProblemDetailsFactory.CreateValidationProblemDetails(
            HttpContext, modelState, status, ReasonPhrases.GetReasonPhrase(status), detail: first.Message);

        foreach (var (key, value) in result.Errors.Where(error => error.Extensions is not null).SelectMany(error => error.Extensions!))
        {
            problem.Extensions[key] = value;
        }

        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static int StatusFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
