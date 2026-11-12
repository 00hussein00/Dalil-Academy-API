using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DalilAcadime.InfraStructures.Migrations
{
    public partial class LinkExistingStudentsToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO \"Users\" (\"FullName\", \"Email\", \"PasswordHash\", \"Role\")
                SELECT s.\"FullName\", s.\"Email\", s.\"PasswordHash\", 'Student'
                FROM \"Students\" s
                WHERE s.\"UserId\" IS NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM \"Users\" u
                      WHERE u.\"Email\" = s.\"Email\"
                  );
                """);

            migrationBuilder.Sql("""
                UPDATE \"Students\" s
                SET \"UserId\" = u.\"Id\",
                    \"Role\" = 'Student'
                FROM \"Users\" u
                WHERE s.\"UserId\" IS NULL
                  AND u.\"Email\" = s.\"Email\";
                """);

            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId");
        }
    }
}
