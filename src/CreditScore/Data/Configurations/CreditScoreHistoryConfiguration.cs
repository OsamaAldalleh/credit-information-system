using System.Text.Json;
using CreditScore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditScore.Data.Configurations;

public class CreditScoreHistoryConfiguration : IEntityTypeConfiguration<CreditScoreHistory>
{
    public void Configure(EntityTypeBuilder<CreditScoreHistory> builder)
    {
        builder.ToTable("credit_score_history");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.CivilId).HasMaxLength(64);
        builder.Property(h => h.CalculatedAt).HasDefaultValueSql("now()");
        builder.Property(h => h.Grade)
            .HasMaxLength(1)
            .HasConversion(v => v.ToString(), v => Enum.Parse<CreditGrade>(v, true));
        builder.Property(h => h.PreviousGrade)
            .HasMaxLength(1)
            .HasConversion(v => v!.Value.ToString(), v => Enum.Parse<CreditGrade>(v, true));
        // Stored as LOAN_CREATED, NIGHTLY_JOB... to match how the API writes enums.
        builder.Property(h => h.Trigger)
            .HasMaxLength(32)
            .HasConversion(
                v => JsonNamingPolicy.SnakeCaseUpper.ConvertName(v.ToString()),
                v => Enum.Parse<CreditScoreTrigger>(v.Replace("_", ""), true));

        builder.HasIndex(h => new { h.CivilId, h.CalculatedAt });
    }
}
