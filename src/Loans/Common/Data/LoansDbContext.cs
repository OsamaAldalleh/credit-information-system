using Loans.Base.Models;
using Loans.Payments.Models;
using Microsoft.EntityFrameworkCore;

namespace Loans.Common.Data;

public class LoansDbContext(DbContextOptions<LoansDbContext> options) : DbContext(options)
{
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanPayment> loanPayments => Set<LoanPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoansDbContext).Assembly);
    }
}
