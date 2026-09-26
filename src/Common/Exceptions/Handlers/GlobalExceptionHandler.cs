using Common.Api.Responses;
using Common.Exceptions.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Exceptions.Handlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The client disconnected: there is nobody to answer and nothing went wrong on our side.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        var (statusCode, body) = exception switch
        {
            ServiceException ex => (ex.HttpStatus, ApiResponseBuilder.Failure(ex.ErrorCode, ex.ErrorDescription, ex.ValidationErrors, ex.ErrorDetails)),
            BadHttpRequestException ex => (ex.StatusCode, Failure(GenericErrors.FromStatusCode(ex.StatusCode))),
            _ => (StatusCodes.Status500InternalServerError, Failure(GenericErrors.InternalError))
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Request {Method} {Path} failed with {StatusCode}: {Message}", httpContext.Request.Method, httpContext.Request.Path, statusCode, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);
        return true;
    }

    private static ApiResponse<object> Failure(ServiceError error) =>
        ApiResponseBuilder.Failure(error.ErrorCode, error.ErrorDescription);
}
