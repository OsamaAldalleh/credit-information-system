namespace Loans.Payments.Models;

public class LoanPayment
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public required string CivilId { get; set; }
    public Guid InstitutionId { get; set; }
    public required string PaymentReference { get; set; }
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}