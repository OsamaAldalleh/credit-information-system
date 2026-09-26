using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Api.Responses;

public static class ApiResponseBuilder
{
    public static ObjectResult Ok<T>(T body) =>
        new(ApiResponse<T>.Success(body)) { StatusCode = StatusCodes.Status200OK };

    public static CreatedResult Created<T>(string location, T body) =>
        new(location, ApiResponse<T>.Success(body));

    public static NoContentResult NoContent() => new();

    // Failures are only built by the exception handlers; services signal errors by throwing ServiceException.
    internal static ApiResponse<object> Failure(
        string errorCode,
        string errorDescription,
        IReadOnlyList<ValidationError>? validationErrors = null,
        object? errorDetails = null) =>
        ApiResponse<object>.Failure(errorCode, errorDescription, validationErrors, errorDetails);
}
