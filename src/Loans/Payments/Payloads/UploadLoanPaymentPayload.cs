using System.ComponentModel.DataAnnotations;

namespace Loans.Payments.Payloads;

public class UploadLoanPaymentPayload
{
    [Required]
    [StringLength(64)]
    public string? PaymentReference {get; set;}

    [Required]
    public DateOnly? PaymentDate { get; set; }

    [Required]
    public decimal? Amount { get; set; }
}