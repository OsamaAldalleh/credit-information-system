using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Data.Configurations;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("institutions");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name).HasMaxLength(255);
        builder.Property(i => i.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(i => i.Type)
            .HasMaxLength(16)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<InstitutionType>(v, true));

        builder.HasIndex(i => i.Name).IsUnique();
    }
}
