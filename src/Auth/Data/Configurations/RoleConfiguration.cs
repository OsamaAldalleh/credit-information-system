using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasColumnName("role").HasMaxLength(32);
        builder.Property(r => r.Description).HasMaxLength(255);
        builder.Property(r => r.InstitutionType)
            .HasMaxLength(16)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<InstitutionType>(v, true));

        builder.HasIndex(r => r.Name).IsUnique();
    }
}
