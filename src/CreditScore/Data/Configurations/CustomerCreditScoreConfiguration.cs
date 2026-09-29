using CreditScore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditScore.Data.Configurations;

public class CustomerCreditScoreConfiguration : IEntityTypeConfiguration<CustomerCreditScore>
{
    public void Configure(EntityTypeBuilder<CustomerCreditScore> builder)
    {
        builder.ToTable("credit_scores");
        builder.HasKey(c => c.CivilId);

        builder.Property(c => c.CivilId).HasMaxLength(64);
        builder.Property(c => c.CalculatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.Grade)
            .HasMaxLength(1)
            .HasConversion(v => v.ToString(), v => Enum.Parse<CreditGrade>(v, true));
    }
}
