namespace Loans.Common.Clients.Customers;

// Only the fields Loans needs from the Customers service; services share an API contract, not code.
public sealed record CustomerSummary(string CivilId, CustomerLoanEligibility LoanEligibility);

public enum CustomerLoanEligibility
{
    Eligible,
    Blocked
}
