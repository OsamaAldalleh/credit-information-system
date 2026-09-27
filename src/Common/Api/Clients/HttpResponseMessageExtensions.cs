using System.Text.Json;

namespace Common.Api.Clients;

public static class HttpResponseMessageExtensions
{
    // Services answer with the ApiResponse envelope; callers only need what is inside response_body.
    public static async Task<T?> ReadResponseBodyAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return document.RootElement.TryGetProperty("response_body", out var body)
            ? body.Deserialize<T>(ApiJson.Options)
            : default;
    }
}
