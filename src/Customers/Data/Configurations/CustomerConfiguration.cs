using Customers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.CivilId);

        builder.Property(c => c.CivilId).HasMaxLength(64);
        builder.Property(c => c.FullName).HasMaxLength(255);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.LoanEligibility)
            .HasMaxLength(32)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<LoanEligibility>(v, true));
        
    }

}