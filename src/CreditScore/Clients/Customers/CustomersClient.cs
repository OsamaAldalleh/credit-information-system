using System.Net;
using Common.Exceptions;
using CreditScore.Errors;

namespace CreditScore.Clients.Customers;

public sealed class CustomersClient(HttpClient httpClient)
{
    public async Task<bool> CustomerExistsAsync(string civilId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"api/customers/{Uri.EscapeDataString(civilId)}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw ServiceException.ServiceUnavailable(CreditScoreErrors.CustomersServiceUnavailable);
        }
    }
}
