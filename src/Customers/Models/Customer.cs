namespace Customers.Models;

public class Customer
{
    public required string CivilId { get; set; }
    public required string FullName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public LoanEligibility LoanEligibility { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}