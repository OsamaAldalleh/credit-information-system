using System.Net;
using Common.Api.Clients;
using Common.Exceptions;
using Loans.Base.Errors;

namespace Loans.Base.Clients;

public sealed class LitigationsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<Guid>?> GetLegalLoanIdsAsync(string civilId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/litigations/{civilId}/legal-loans/ids", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            response.EnsureSuccessStatusCode();
            return await response.ReadResponseBodyAsync<IReadOnlyList<Guid>>(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw ServiceException.ServiceUnavailable(LoanErrors.LitigationsServiceUnavailable);
        }
    }
}