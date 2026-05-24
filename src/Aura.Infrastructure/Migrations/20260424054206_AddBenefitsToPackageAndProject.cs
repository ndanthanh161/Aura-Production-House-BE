using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefitsToPackageAndProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Packages",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            // Fix các row cũ có giá trị empty string (không phải JSON hợp lệ)
            migrationBuilder.Sql("UPDATE \"Projects\" SET \"Benefits\" = '[]' WHERE \"Benefits\" = '' OR \"Benefits\" IS NULL;");
            migrationBuilder.Sql("UPDATE \"Packages\" SET \"Benefits\" = '[]' WHERE \"Benefits\" = '' OR \"Benefits\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Packages");
        }
    }
}
