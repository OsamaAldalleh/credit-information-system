using CreditScore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditScore.Data;

public class CreditScoreDbContext(DbContextOptions<CreditScoreDbContext> options) : DbContext(options)
{
    public DbSet<CustomerCreditScore> CreditScores => Set<CustomerCreditScore>();
    public DbSet<CreditScoreHistory> CreditScoreHistory => Set<CreditScoreHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreditScoreDbContext).Assembly);
    }
}
