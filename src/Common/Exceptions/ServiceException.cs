using System.Globalization;
using Common.Api.Responses;
using Common.Exceptions.Errors;
using Microsoft.AspNetCore.Http;

namespace Common.Exceptions;

public sealed class ServiceException : Exception
{
    public int HttpStatus { get; }
    public string ErrorCode { get; }
    public string ErrorDescription { get; }
    public IReadOnlyList<ValidationError>? ValidationErrors { get; }
    public object? ErrorDetails { get; private set; }

    private ServiceException(int httpStatus, ServiceError error, object?[] args, IReadOnlyList<ValidationError>? validationErrors = null)
        : base(Format(error, args))
    {
        HttpStatus = httpStatus;
        ErrorCode = error.ErrorCode;
        ErrorDescription = Message;
        ValidationErrors = validationErrors;
    }

    public static ServiceException BadRequest(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status400BadRequest, error, args);

    public static ServiceException Unauthorized(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status401Unauthorized, error, args);

    public static ServiceException Forbidden(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status403Forbidden, error, args);

    public static ServiceException NotFound(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status404NotFound, error, args);

    public static ServiceException Conflict(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status409Conflict, error, args);

    public static ServiceException UnprocessableEntity(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status422UnprocessableEntity, error, args);

    public static ServiceException InternalError(ServiceError error, params object?[] args) =>
        new(StatusCodes.Status500InternalServerError, error, args);

    public static ServiceException ValidationFailed(IReadOnlyList<ValidationError> validationErrors) =>
        new(StatusCodes.Status400BadRequest, GenericErrors.ValidationFailed, [], validationErrors);

    // A separate call rather than a factory parameter, since the factories' params args would swallow it.
    public ServiceException WithDetails(object errorDetails)
    {
        ErrorDetails = errorDetails;
        return this;
    }

    private static string Format(ServiceError error, object?[] args) =>
        args.Length == 0 ? error.ErrorDescription : string.Format(CultureInfo.InvariantCulture, error.ErrorDescription, args);
}
