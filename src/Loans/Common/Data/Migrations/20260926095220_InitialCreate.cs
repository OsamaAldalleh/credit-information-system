using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loans.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_reference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    first_due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    tenor = table.Column<int>(type: "integer", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    installment_amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    payment_frequency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    closure_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loans", x => x.id);
                    table.CheckConstraint("ck_loans_closed_at_matches_status", "(status = 'CLOSED') = (closed_at IS NOT NULL)");
                    table.CheckConstraint("ck_loans_first_due_after_start", "first_due_date >= start_date");
                });

            migrationBuilder.CreateTable(
                name: "loan_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_reference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loan_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_loan_payments_loans_loan_id",
                        column: x => x.loan_id,
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_loan_payments_civil_id_payment_date",
                table: "loan_payments",
                columns: new[] { "civil_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_loan_payments_institution_id_payment_reference",
                table: "loan_payments",
                columns: new[] { "institution_id", "payment_reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loan_payments_loan_id_payment_date",
                table: "loan_payments",
                columns: new[] { "loan_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_loans_civil_id",
                table: "loans",
                column: "civil_id");

            migrationBuilder.CreateIndex(
                name: "ix_loans_institution_id_external_reference",
                table: "loans",
                columns: new[] { "institution_id", "external_reference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loan_payments");

            migrationBuilder.DropTable(
                name: "loans");
        }
    }
}
