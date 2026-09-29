namespace CreditScore.Clients.Loans;

public sealed record LoanCreditSummary(Guid LoanId, decimal Amount, LoanStatus Status, int OverdueInstallments);

public enum LoanStatus
{
    Open,
    Closed
}
