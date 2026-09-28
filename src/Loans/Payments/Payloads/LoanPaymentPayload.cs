namespace Loans.Payments.Payloads;

public class LoanPaymentPayload
{
    public Guid? Id { get; set; }
    public Guid? LoanId { get; set; }
    public string? CivilId { get; set; }
    public string? PaymentReference {get; set;}
    public DateOnly? PaymentDate { get; set; }
    public decimal? Amount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}