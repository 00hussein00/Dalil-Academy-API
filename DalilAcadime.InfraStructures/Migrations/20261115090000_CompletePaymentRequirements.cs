using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DalilAcadime.InfraStructures.Migrations
{
    /// <inheritdoc />
    public partial class CompletePaymentRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Amount\" TYPE numeric(10,2);");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Status\" TYPE text USING CASE \"Status\" WHEN 0 THEN 'Created' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Failed' ELSE \"Status\"::text END;");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"CreatedAt\" SET DEFAULT now();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Status\" TYPE integer USING CASE \"Status\" WHEN 'Created' THEN 0 WHEN 'Completed' THEN 1 WHEN 'Failed' THEN 2 ELSE \"Status\"::integer END;");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Amount\" TYPE numeric(6,2);");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"CreatedAt\" DROP DEFAULT;");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Payments");
        }
    }
}
