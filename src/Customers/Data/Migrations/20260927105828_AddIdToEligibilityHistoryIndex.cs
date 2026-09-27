using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customers.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToEligibilityHistoryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customer_loan_eligibility_history_civil_id_created_at",
                table: "customer_loan_eligibility_history");

            migrationBuilder.CreateIndex(
                name: "ix_customer_loan_eligibility_history_civil_id_created_at_id",
                table: "customer_loan_eligibility_history",
                columns: new[] { "civil_id", "created_at", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customer_loan_eligibility_history_civil_id_created_at_id",
                table: "customer_loan_eligibility_history");

            migrationBuilder.CreateIndex(
                name: "ix_customer_loan_eligibility_history_civil_id_created_at",
                table: "customer_loan_eligibility_history",
                columns: new[] { "civil_id", "created_at" });
        }
    }
}
