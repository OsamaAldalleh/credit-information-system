namespace Loans.Models;

public class Loan
{
    public Guid Id { get; set; }
    public required string CivilId { get; set; }
    public Guid InstitutionId { get; set; }
    public required string ExternalReference { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly FirstDueDate { get; set; }
    public int Tenor { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public PaymentFrequency PaymentFrequency { get; set; }
    public LoanStatus Status { get; set; }
    public string? ClosureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
