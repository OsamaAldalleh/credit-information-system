namespace Loans.Base.Payloads;

public class NextPaymentPayload
{
    public Guid LoanId { get; set; }
    public string? ExternalReference { get; set; }
    public decimal OverdueAmount { get; set; }
    public DateOnly? OverdueSince { get; set; }
    public DateOnly? NextDueDate { get; set; }
    public decimal NextInstallmentAmount { get; set; }
    public decimal TotalDueByNextDate { get; set; }
}
