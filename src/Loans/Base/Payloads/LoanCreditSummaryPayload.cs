using Loans.Base.Models;

namespace Loans.Base.Payloads;

public class LoanCreditSummaryPayload
{
    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
    public LoanStatus Status { get; set; }
    public int OverdueInstallments { get; set; }
}
