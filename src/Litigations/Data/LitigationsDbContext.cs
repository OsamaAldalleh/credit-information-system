using Litigations.Models;
using Microsoft.EntityFrameworkCore;

namespace Litigations.Data;

public class LitigationsDbContext(DbContextOptions<LitigationsDbContext> options) : DbContext(options)
{
    public DbSet<Litigation> Litigations => Set<Litigation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LitigationsDbContext).Assembly);
    }
}
