using Customers.Models;
using Microsoft.EntityFrameworkCore;

namespace Customers.Data;

public static class CustomersSeeder
{
    public static void Seed(CustomersDbContext db)
    {
        Customer[] customers =
        [
            CreateCustomer("900000000001", "Demo Current Customer", new DateOnly(1990, 1, 15)),
            CreateCustomer("900000000002", "Demo Overdue Customer", new DateOnly(1985, 6, 20)),
            CreateCustomer("900000000003", "Demo Schedule Customer", new DateOnly(1995, 9, 10)),
            CreateCustomer("900000000004", "Demo Blocked Customer", new DateOnly(1988, 3, 25), LoanEligibility.Blocked)
        ];

        var civilIds = customers.Select(c => c.CivilId).ToArray();
        var existingIds = db.Customers.AsNoTracking()
            .Where(c => civilIds.Contains(c.CivilId))
            .Select(c => c.CivilId)
            .ToHashSet();

        db.Customers.AddRange(customers.Where(c => !existingIds.Contains(c.CivilId)));
        db.SaveChanges();
    }

    private static Customer CreateCustomer(string civilId, string name, DateOnly dateOfBirth,
            LoanEligibility eligibility = LoanEligibility.Eligible) => new()
        {
            CivilId = civilId,
            FullName = name,
            DateOfBirth = dateOfBirth,
            LoanEligibility = eligibility,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
