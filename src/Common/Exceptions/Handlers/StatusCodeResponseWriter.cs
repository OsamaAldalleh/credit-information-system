using Common.Api.Responses;
using Common.Exceptions.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Common.Exceptions.Handlers;

// Covers error responses ASP.NET produces without throwing (unknown route, wrong method, 401/403 from auth),
// which never reach GlobalExceptionHandler.
public static class StatusCodeResponseWriter
{
    public static async Task WriteAsync(StatusCodeContext context)
    {
        var response = context.HttpContext.Response;
        var error = GenericErrors.FromStatusCode(response.StatusCode);

        await response.WriteAsJsonAsync(
            ApiResponseBuilder.Failure(error.ErrorCode, error.ErrorDescription),
            context.HttpContext.RequestAborted);
    }
}
