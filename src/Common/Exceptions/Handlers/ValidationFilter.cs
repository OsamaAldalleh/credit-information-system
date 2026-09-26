using Common.Api.Responses;
using Common.Exceptions.Errors;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Common.Exceptions.Handlers;

// Replaces [ApiController]'s automatic 400 so validation failures go through GlobalExceptionHandler like every other error.
public sealed class ValidationFilter : IActionFilter
{
    private const string InvalidValueMessage = "Invalid value";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var invalidEntries = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToList();

        // JSON parsing failures are keyed by JSON path ("$", "$.amount"). When present, the model was never built,
        // so any other entries are noise. The raw parser messages expose .NET types, hence the generic message.
        var jsonEntries = invalidEntries.Where(entry => entry.Key.StartsWith('$')).ToList();
        if (jsonEntries.Count > 0)
        {
            var fieldErrors = jsonEntries
                .Where(entry => entry.Key.StartsWith("$."))
                .Select(entry => new ValidationError(entry.Key[2..], InvalidValueMessage))
                .ToList();

            throw fieldErrors.Count > 0
                ? ServiceException.ValidationFailed(fieldErrors)
                : ServiceException.BadRequest(GenericErrors.MalformedRequest);
        }

        // An empty key is the request body itself, e.g. an empty body.
        if (invalidEntries.Any(entry => entry.Key.Length == 0))
        {
            throw ServiceException.BadRequest(GenericErrors.MalformedRequest);
        }

        var validationErrors = invalidEntries
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ValidationError(
                entry.Key,
                string.IsNullOrWhiteSpace(error.ErrorMessage) ? InvalidValueMessage : error.ErrorMessage)))
            .ToList();

        throw ServiceException.ValidationFailed(validationErrors);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
