using Common.Api.Clients;
using Common.Exceptions;
using CreditScore.Errors;

namespace CreditScore.Clients.Loans;

public sealed class LoansClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<LoanCreditSummary>> GetCreditSummaryAsync(string civilId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/loans/{Uri.EscapeDataString(civilId)}/credit-summary", cancellationToken);

            response.EnsureSuccessStatusCode();
            return await response.ReadResponseBodyAsync<IReadOnlyList<LoanCreditSummary>>(cancellationToken) ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw ServiceException.ServiceUnavailable(CreditScoreErrors.LoansServiceUnavailable);
        }
    }
}
