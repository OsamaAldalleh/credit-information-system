using Customers.Models;
using System.ComponentModel.DataAnnotations;

namespace Customers.Payloads;

public class UpdateEligibilityPayload
{
    [Required]
    [EnumDataType(typeof(LoanEligibility))]
    public LoanEligibility? LoanEligibility { get; set; }

    [Required]
    [StringLength(255)]
    public string? Reason { get; set; }
}