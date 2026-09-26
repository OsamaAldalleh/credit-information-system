using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Common.Api.Responses;

// Property names are pinned with attributes so the envelope keeps the same shape regardless of each service's JSON settings.
public sealed class ApiResponse<T>
{
    [JsonPropertyName("msg_id")]
    public string MsgId { get; }

    [JsonPropertyName("response_date_time")]
    public DateTimeOffset ResponseDateTime { get; }

    [JsonPropertyName("status")]
    public ResponseStatus Status { get; }

    [JsonPropertyName("response_body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? ResponseBody { get; private init; }

    [JsonPropertyName("validation_errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ValidationError>? ValidationErrors { get; private init; }

    [JsonPropertyName("error_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; private init; }

    [JsonPropertyName("error_description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorDescription { get; private init; }

    [JsonPropertyName("error_details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ErrorDetails { get; private init; }

    private ApiResponse(ResponseStatus status)
    {
        // The request's trace id, so a msg_id reported by a client can be found in the logs.
        MsgId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        ResponseDateTime = DateTimeOffset.UtcNow;
        Status = status;
    }

    internal static ApiResponse<T> Success(T? body) =>
        new(ResponseStatus.Success) { ResponseBody = body };

    internal static ApiResponse<T> Failure(string errorCode, string errorDescription, IReadOnlyList<ValidationError>? validationErrors, object? errorDetails) =>
        new(ResponseStatus.Failure)
        {
            ErrorCode = errorCode,
            ErrorDescription = errorDescription,
            ValidationErrors = validationErrors,
            ErrorDetails = errorDetails
        };
}
