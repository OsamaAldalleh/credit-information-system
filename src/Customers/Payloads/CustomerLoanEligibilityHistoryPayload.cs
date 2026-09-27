using Customers.Models;

namespace Customers.Payloads;

public class CustomerLoanEligibilityHistoryPayload
{
    public Guid Id { get; set; }
    public LoanEligibility LoanEligibility { get; set; }
    public string? Reason { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}