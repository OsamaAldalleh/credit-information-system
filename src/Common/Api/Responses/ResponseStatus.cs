using System.Text.Json.Serialization;

namespace Common.Api.Responses;

[JsonConverter(typeof(JsonStringEnumConverter<ResponseStatus>))]
public enum ResponseStatus
{
    [JsonStringEnumMemberName("SUCCESS")]
    Success,

    [JsonStringEnumMemberName("FAILURE")]
    Failure
}
