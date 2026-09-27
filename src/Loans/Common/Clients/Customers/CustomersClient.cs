using System.Net;
using Common.Api.Clients;
using Common.Exceptions;
using Loans.Base.Errors;

namespace Loans.Common.Clients.Customers;

// Typed client: HttpClient is created and pooled by IHttpClientFactory (registered in Program.cs).
public sealed class CustomersClient(HttpClient httpClient)
{
    public async Task<CustomerSummary?> GetCustomerAsync(string civilId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/customers/{Uri.EscapeDataString(civilId)}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.ReadResponseBodyAsync<CustomerSummary>(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // Customers is down, unreachable, failing or too slow: report it as our dependency's outage, not our bug.
            throw ServiceException.ServiceUnavailable(LoanErrors.CustomersServiceUnavailable);
        }
    }
}
