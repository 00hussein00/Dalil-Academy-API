using DalilAcadime.InfraStructures.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DalilAcadime.InfraStructures.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261115120000_FixEnrollmentStatusColumn")]
    public partial class FixEnrollmentStatusColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Enrollments\" ALTER COLUMN \"Status\" TYPE text USING CASE \"Status\" WHEN 0 THEN 'Pending' WHEN 1 THEN 'Paid' ELSE \"Status\"::text END;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Enrollments\" ALTER COLUMN \"Status\" TYPE integer USING CASE \"Status\" WHEN 'Pending' THEN 0 WHEN 'Paid' THEN 1 ELSE \"Status\"::integer END;");
        }
    }
}
