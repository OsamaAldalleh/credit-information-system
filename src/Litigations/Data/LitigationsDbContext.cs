using Litigations.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Litigations.Data;

public class LitigationsDbContext(DbContextOptions<LitigationsDbContext> options) : DbContext(options)
{
    public DbSet<Litigation> Litigations => Set<Litigation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LitigationsDbContext).Assembly);

        // Outbox: messages are saved in the same transaction as the data, then sent to RabbitMQ.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
