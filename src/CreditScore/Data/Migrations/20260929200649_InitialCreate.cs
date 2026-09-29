using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditScore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_score_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    grade = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    previous_grade = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    active_loans = table.Column<int>(type: "integer", nullable: false),
                    delinquencies = table.Column<int>(type: "integer", nullable: false),
                    loans_in_litigation = table.Column<int>(type: "integer", nullable: false),
                    recent_guilty_verdicts = table.Column<int>(type: "integer", nullable: false),
                    trigger = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credit_score_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "credit_scores",
                columns: table => new
                {
                    civil_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    grade = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    active_loans = table.Column<int>(type: "integer", nullable: false),
                    delinquencies = table.Column<int>(type: "integer", nullable: false),
                    loans_in_litigation = table.Column<int>(type: "integer", nullable: false),
                    recent_guilty_verdicts = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credit_scores", x => x.civil_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_credit_score_history_civil_id_calculated_at",
                table: "credit_score_history",
                columns: new[] { "civil_id", "calculated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_score_history");

            migrationBuilder.DropTable(
                name: "credit_scores");
        }
    }
}
