using DalilAcadime.InfraStructures.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DalilAcadime.InfraStructures.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261115110000_RepairPaymentColumns")]
    public partial class RepairPaymentColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ADD COLUMN IF NOT EXISTS \"Currency\" character varying(3) NOT NULL DEFAULT 'USD';");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ADD COLUMN IF NOT EXISTS \"PaidAt\" timestamp with time zone NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"Amount\" TYPE numeric(10,2);");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" ALTER COLUMN \"CreatedAt\" SET DEFAULT now();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" DROP COLUMN IF EXISTS \"PaidAt\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"Payments\" DROP COLUMN IF EXISTS \"Currency\";");
        }
    }
}
