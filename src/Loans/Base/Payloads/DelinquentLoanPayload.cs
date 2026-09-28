namespace Loans.Base.Payloads;

public class DelinquentLoanPayload
{
    public Guid LoanId { get; set; }
    public string? ExternalReference { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OverdueInstallments { get; set; }
    public DateOnly? OverdueSince { get; set; }
    public int DaysPastDue { get; set; }
    public string? DelinquencyBucket { get; set; }
}
