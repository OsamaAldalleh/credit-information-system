using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Litigations.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "litigations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_case_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    filed_date = table.Column<DateOnly>(type: "date", nullable: false),
                    verdict_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_litigations", x => x.id);
                    table.CheckConstraint("ck_litigations_verdict_date_matches_status", "(status = 'PENDING') = (verdict_date IS NULL)");
                });

            migrationBuilder.CreateIndex(
                name: "ix_litigations_civil_id",
                table: "litigations",
                column: "civil_id");

            migrationBuilder.CreateIndex(
                name: "ix_litigations_court_case_number_loan_id",
                table: "litigations",
                columns: new[] { "court_case_number", "loan_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_litigations_loan_id",
                table: "litigations",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "ix_litigations_loan_id_guilty",
                table: "litigations",
                column: "loan_id",
                unique: true,
                filter: "status = 'GUILTY'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "litigations");
        }
    }
}
