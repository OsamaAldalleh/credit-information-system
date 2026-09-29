using System.ComponentModel.DataAnnotations;

namespace Loans.Base.Payloads;

public class CloseLoanPayload
{
    [Required]
    [StringLength(255)]
    public string? ClosureReason { get; set; }
}
