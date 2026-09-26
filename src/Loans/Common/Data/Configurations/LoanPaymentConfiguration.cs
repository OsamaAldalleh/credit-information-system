using Loans.Base.Models;
using Loans.Payments.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loans.Common.Data.Configurations;

public class LoanPaymentConfiguration : IEntityTypeConfiguration<LoanPayment>
{
    public void Configure(EntityTypeBuilder<LoanPayment> builder)
    {
        builder.ToTable("loan_payments");
        builder.HasKey(lp => lp.Id);
        builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lp => lp.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Property(lp => lp.PaymentReference).HasMaxLength(64);
        builder.Property(lp => lp.CivilId).HasMaxLength(64);
        builder.Property(lp => lp.Amount).HasPrecision(18, 3);
        builder.Property(lp => lp.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(lp => new { lp.InstitutionId, lp.PaymentReference }).IsUnique();
        builder.HasIndex(lp => new { lp.LoanId, lp.PaymentDate });
        builder.HasIndex(lp => new { lp.CivilId, lp.PaymentDate });
    }
}