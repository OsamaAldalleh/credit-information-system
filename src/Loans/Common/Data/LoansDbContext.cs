using Loans.Base.Models;
using Loans.Payments.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Loans.Common.Data;

public class LoansDbContext(DbContextOptions<LoansDbContext> options) : DbContext(options)
{
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanPayment> LoanPayments => Set<LoanPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoansDbContext).Assembly);

        // Outbox: messages are saved in the same transaction as the data, then sent to RabbitMQ.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
