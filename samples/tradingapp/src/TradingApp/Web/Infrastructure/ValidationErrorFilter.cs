using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TradingApp.Web.Infrastructure;

/// <summary>
/// Returns 400 with { message, errors } when ModelState is invalid.
/// </summary>
public sealed class ValidationErrorFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Result != null) return;
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                .ToArray();
            context.Result = new BadRequestObjectResult(new ApiErrorResponse("Validation failed.", errors));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
