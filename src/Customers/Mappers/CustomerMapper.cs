using Customers.Models;
using Customers.Payloads;

namespace Customers.Mappers;

public static class CustomerMapper
{
    public static CustomerPayload ToPayload(this Customer customer) => new()
    {
        CivilId = customer.CivilId,
        FullName = customer.FullName,
        DateOfBirth = customer.DateOfBirth,
        LoanEligibility = customer.LoanEligibility
    };

    // Payload fields are non-null here: [Required] rejects the request before it reaches the service.
    public static Customer ToEntity(this CustomerPayload payload)
    {
        var now = DateTimeOffset.UtcNow;

        return new Customer
        {
            CivilId = payload.CivilId!,
            FullName = payload.FullName!,
            DateOfBirth = payload.DateOfBirth!.Value,
            // New customers can take loans; becoming ineligible only happens through the block flow.
            LoanEligibility = LoanEligibility.Eligible,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static CustomerLoanEligibilityHistoryPayload ToPayload(this CustomerLoanEligibilityHistory history)
    {
        return new()
        {
            Id = history.Id,
            LoanEligibility = history.LoanEligibility,
            Reason = history.Reason,
            ChangedBy = history.ChangedBy,
            CreatedAt = history.CreatedAt
        };
    }
}
