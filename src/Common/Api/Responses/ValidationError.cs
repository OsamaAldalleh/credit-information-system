using System.Text.Json.Serialization;

namespace Common.Api.Responses;

public sealed record ValidationError(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")] string Message);
