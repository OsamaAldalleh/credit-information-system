using System.ComponentModel.DataAnnotations;
using Litigations.Models;

namespace Litigations.Payloads;

public class CreateLitigationPayload
{
    [Required]
    public Guid? LoanId { get; set; }

    [Required]
    [StringLength(64)]
    public string? CourtCaseNumber { get; set; }

    [Required]
    public DateOnly? FiledDate { get; set; }

    // Optional: a case can be registered as PENDING or with its verdict already known.
    [EnumDataType(typeof(LitigationStatus))]
    public LitigationStatus? Status { get; set; }

    public DateOnly? VerdictDate { get; set; }
}
