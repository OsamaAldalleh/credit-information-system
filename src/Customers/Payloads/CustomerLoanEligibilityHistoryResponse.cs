using Common.Api.Responses;

namespace Customers.Payloads;

public class CustomerLoanEligibilityHistoryResponse
{
    public required string CivilId { get; set; }
    public required PageResult<CustomerLoanEligibilityHistoryPayload> Result { get; set;}
}