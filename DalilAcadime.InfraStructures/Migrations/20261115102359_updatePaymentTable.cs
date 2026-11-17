using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DalilAcadime.InfraStructures.Migrations
{
    /// <inheritdoc />
    public partial class updatePaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Status\" TYPE text USING CASE \"Status\" WHEN 0 THEN 'Created' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Failed' ELSE \"Status\"::text END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Status\" TYPE integer USING CASE \"Status\" WHEN 'Created' THEN 0 WHEN 'Completed' THEN 1 WHEN 'Failed' THEN 2 ELSE \"Status\"::integer END;");
        }
    }
}
