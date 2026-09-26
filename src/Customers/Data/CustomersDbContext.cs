using Customers.Models;
using Microsoft.EntityFrameworkCore;

namespace Customers.Data;

public class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerLoanEligibilityHistory> CustomerLoanEligibilityHistory => Set<CustomerLoanEligibilityHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
    }
}