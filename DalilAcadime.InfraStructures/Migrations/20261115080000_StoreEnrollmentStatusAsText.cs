using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DalilAcadime.InfraStructures.Migrations
{
    /// <inheritdoc />
    public partial class StoreEnrollmentStatusAsText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Enrollments\" ALTER COLUMN \"Status\" TYPE text USING CASE \"Status\" WHEN 0 THEN 'Pending' WHEN 1 THEN 'Paid' ELSE \"Status\"::text END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Enrollments\" ALTER COLUMN \"Status\" TYPE integer USING CASE \"Status\" WHEN 'Pending' THEN 0 WHEN 'Paid' THEN 1 ELSE \"Status\"::integer END;");
        }
    }
}
