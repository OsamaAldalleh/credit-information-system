using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.HasOne<Institution>()
            .WithMany()
            .HasForeignKey(u => u.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(u => u.Username).HasMaxLength(64);
        builder.Property(u => u.PasswordHash).HasMaxLength(255);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(u => u.Username).IsUnique();
    }
}
