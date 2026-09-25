using Loans.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Data;

public class LoansDbContext(DbContextOptions<LoansDbContext> options) : DbContext(options)
{
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoansDbContext).Assembly);
    }
}
