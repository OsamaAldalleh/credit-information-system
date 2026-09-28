using System.Net;
using Common.Api.Clients;
using Common.Exceptions;
using Litigations.Errors;

namespace Litigations.Clients.Loans;

public sealed class LoansClient(HttpClient httpClient)
{
    public async Task<LoanSummary?> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/loans/{loanId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.ReadResponseBodyAsync<LoanSummary>(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw ServiceException.ServiceUnavailable(LitigationErrors.LoansServiceUnavailable);
        }
    }
}
