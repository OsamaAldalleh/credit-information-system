using System.ComponentModel.DataAnnotations;
using Customers.Models;

namespace Customers.Payloads;

public class CustomerPayload
{
    [Required]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "civil_id must be exactly 12 digits")]
    public string? CivilId { get; set; }

    [Required]
    [StringLength(255)]
    public string? FullName { get; set; }

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    public LoanEligibility LoanEligibility { get; internal set; }
}