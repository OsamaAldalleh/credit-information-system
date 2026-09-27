using System.ComponentModel.DataAnnotations;
using Common.Api.Validation;
using Loans.Base.Models;

namespace Loans.Base.Payloads;

public class CreateLoanPayload
{
    [Required]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")]
    public string? CivilId { get; set; }

    [Required]
    [StringLength(64)]
    public string? ExternalReference { get; set; }

    [Required]
    public DateOnly? StartDate { get; set; }

    [Required]
    public DateOnly? FirstDueDate { get; set; }

    [Required, Positive]
    public int? Tenor { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal? Rate { get; set; }

    [Required, Positive]
    public decimal? Amount { get; set; }

    [Required, Positive]
    public decimal? InstallmentAmount { get; set; }

    [Required]
    [EnumDataType(typeof(PaymentFrequency))]
    public PaymentFrequency? PaymentFrequency { get; set; }
}