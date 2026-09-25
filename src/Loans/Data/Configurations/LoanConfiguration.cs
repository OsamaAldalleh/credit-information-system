using Loans.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loans.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans", t =>
        {
            t.HasCheckConstraint("ck_loans_first_due_after_start", "first_due_date >= start_date");
            t.HasCheckConstraint("ck_loans_closed_at_matches_status", "(status = 'CLOSED') = (closed_at IS NOT NULL)");
        });

        builder.HasKey(l => l.Id);

        builder.Property(l => l.CivilId).HasMaxLength(64);
        builder.Property(l => l.ExternalReference).HasMaxLength(64);
        builder.Property(l => l.Rate).HasPrecision(7, 4);
        builder.Property(l => l.Amount).HasPrecision(18, 3);
        builder.Property(l => l.InstallmentAmount).HasPrecision(18, 3);
        builder.Property(l => l.ClosureReason).HasMaxLength(255);
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("now()");

        // Enums stored as uppercase text (WEEKLY, OPEN) so the data stays readable in plain SQL.
        builder.Property(l => l.PaymentFrequency)
            .HasMaxLength(16)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<PaymentFrequency>(v, true));
        builder.Property(l => l.Status)
            .HasMaxLength(32)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<LoanStatus>(v, true));

        builder.HasIndex(l => new { l.InstitutionId, l.ExternalReference }).IsUnique();
        builder.HasIndex(l => l.CivilId);
    }
}
