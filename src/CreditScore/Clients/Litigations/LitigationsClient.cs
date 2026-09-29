using Common.Api.Clients;
using Common.Api.Responses;
using Common.Exceptions;
using CreditScore.Errors;

namespace CreditScore.Clients.Litigations;

public sealed class LitigationsClient(HttpClient httpClient)
{
    private const int PageSize = 100;

    public async Task<IReadOnlyList<Guid>> GetLegalLoanIdsAsync(string civilId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/litigations/{Uri.EscapeDataString(civilId)}/legal-loans/ids", cancellationToken);

            response.EnsureSuccessStatusCode();
            return await response.ReadResponseBodyAsync<IReadOnlyList<Guid>>(cancellationToken) ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw ServiceException.ServiceUnavailable(CreditScoreErrors.LitigationsServiceUnavailable);
        }
    }

    public async Task<IReadOnlyList<GuiltyVerdict>> GetGuiltyVerdictsAsync(string civilId, CancellationToken cancellationToken = default)
    {
        try
        {
            var verdicts = new List<GuiltyVerdict>();
            var page = 0;
            PageResult<GuiltyVerdict>? result;

            do
            {
                using var response = await httpClient.GetAsync(
                    $"api/litigations/{Uri.EscapeDataString(civilId)}?status=GUILTY&page={page}&pageSize={PageSize}",
                    cancellationToken);

                response.EnsureSuccessStatusCode();
                result = await response.ReadResponseBodyAsync<PageResult<GuiltyVerdict>>(cancellationToken);
                verdicts.AddRange(result?.Content ?? []);
                page++;
            }
            while (result is not null && page < result.TotalPages);

            return verdicts;
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw ServiceException.ServiceUnavailable(CreditScoreErrors.LitigationsServiceUnavailable);
        }
    }
}
