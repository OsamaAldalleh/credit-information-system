using Microsoft.AspNetCore.Http;

namespace Common.Exceptions.Errors;

public static class GenericErrors
{
    public static readonly ServiceError InternalError = new("GEN-1000", "An unexpected error occurred");
    public static readonly ServiceError ValidationFailed = new("GEN-1001", "One or more validation errors occurred");
    public static readonly ServiceError MalformedRequest = new("GEN-1002", "The request body is missing or malformed");
    public static readonly ServiceError Unauthorized = new("GEN-1003", "Authentication is required");
    public static readonly ServiceError Forbidden = new("GEN-1004", "You are not allowed to perform this action");
    public static readonly ServiceError NotFound = new("GEN-1005", "The requested resource was not found");
    public static readonly ServiceError MethodNotAllowed = new("GEN-1006", "The HTTP method is not allowed for this resource");
    public static readonly ServiceError UnsupportedMediaType = new("GEN-1007", "Unsupported content type, use application/json");
    public static readonly ServiceError RequestTooLarge = new("GEN-1008", "The request body is too large");
    public static readonly ServiceError RequestFailed = new("GEN-1009", "The request could not be processed");
    public static readonly ServiceError TooManyRequests = new("GEN-1010", "Too many requests, try again later");

    internal static ServiceError FromStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => MalformedRequest,
        StatusCodes.Status401Unauthorized => Unauthorized,
        StatusCodes.Status403Forbidden => Forbidden,
        StatusCodes.Status404NotFound => NotFound,
        StatusCodes.Status405MethodNotAllowed => MethodNotAllowed,
        StatusCodes.Status413PayloadTooLarge => RequestTooLarge,
        StatusCodes.Status415UnsupportedMediaType => UnsupportedMediaType,
        StatusCodes.Status429TooManyRequests => TooManyRequests,
        >= StatusCodes.Status500InternalServerError => InternalError,
        _ => RequestFailed
    };
}
