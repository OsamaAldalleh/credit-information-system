using Customers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Data.Configurations;

public class CustomerLoanEligibilityHistoryConfiguration : IEntityTypeConfiguration<CustomerLoanEligibilityHistory>
{
    public void Configure(EntityTypeBuilder<CustomerLoanEligibilityHistory> builder)
    {
        builder.ToTable("customer_loan_eligibility_history");
        builder.HasKey(le => le.Id);
        builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(le => le.CivilId);
        
        builder.Property(le => le.CivilId).HasMaxLength(64);
        builder.Property(le => le.Reason).HasMaxLength(255);
        builder.Property(le => le.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(le => le.LoanEligibility)
            .HasMaxLength(32)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<LoanEligibility>(v, true));

        builder.HasIndex(le => new { le.CivilId, le.CreatedAt });
    }
}