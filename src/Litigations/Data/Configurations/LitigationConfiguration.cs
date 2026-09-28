using Litigations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Litigations.Data.Configurations;

public class LitigationConfiguration : IEntityTypeConfiguration<Litigation>
{
    public const string CaseNumberPerLoanIndex = "ix_litigations_court_case_number_loan_id";
    public const string GuiltyVerdictPerLoanIndex = "ix_litigations_loan_id_guilty";

    public void Configure(EntityTypeBuilder<Litigation> builder)
    {
        builder.ToTable("litigations", t =>
        {
            t.HasCheckConstraint("ck_litigations_verdict_date_matches_status", "(status = 'PENDING') = (verdict_date IS NULL)");
        });

        builder.HasKey(l => l.Id);

        builder.Property(l => l.CourtCaseNumber).HasMaxLength(64);
        builder.Property(l => l.CivilId).HasMaxLength(64);
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(l => l.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(l => l.Status)
            .HasMaxLength(16)
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<LitigationStatus>(v, true));

        // Database names are pinned: the service tells unique violations apart by index name.
        builder.HasIndex(l => new { l.CourtCaseNumber, l.LoanId }, CaseNumberPerLoanIndex)
            .HasDatabaseName(CaseNumberPerLoanIndex)
            .IsUnique();
        builder.HasIndex(l => l.LoanId, "ix_litigations_loan_id")
            .HasDatabaseName("ix_litigations_loan_id");
        // A loan can have several cases, but at most one ends in a GUILTY verdict.
        builder.HasIndex(l => l.LoanId, GuiltyVerdictPerLoanIndex)
            .HasDatabaseName(GuiltyVerdictPerLoanIndex)
            .IsUnique()
            .HasFilter("status = 'GUILTY'");
        builder.HasIndex(l => l.CivilId);
    }
}
