using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customers.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    loan_eligibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.civil_id);
                });

            migrationBuilder.CreateTable(
                name: "customer_loan_eligibility_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    loan_eligibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_loan_eligibility_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_loan_eligibility_history_customers_civil_id",
                        column: x => x.civil_id,
                        principalTable: "customers",
                        principalColumn: "civil_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_loan_eligibility_history_civil_id_created_at",
                table: "customer_loan_eligibility_history",
                columns: new[] { "civil_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_loan_eligibility_history");

            migrationBuilder.DropTable(
                name: "customers");
        }
    }
}
