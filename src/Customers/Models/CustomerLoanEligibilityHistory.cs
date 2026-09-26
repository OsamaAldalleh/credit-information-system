namespace Customers.Models;

public class CustomerLoanEligibilityHistory
{
    public Guid Id { get; set; }
    public required string CivilId { get; set; }
    public LoanEligibility LoanEligibility { get; set; }
    public string? Reason { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}