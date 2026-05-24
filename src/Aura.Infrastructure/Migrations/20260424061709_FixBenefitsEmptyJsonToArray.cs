using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBenefitsEmptyJsonToArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix tất cả row có Benefits = '' (empty string không hợp lệ JSON)
            // → chuyển thành '[]' (JSON array rỗng hợp lệ)
            migrationBuilder.Sql("UPDATE \"Packages\" SET \"Benefits\" = '[]' WHERE \"Benefits\" = '' OR \"Benefits\" IS NULL;");
            migrationBuilder.Sql("UPDATE \"Projects\" SET \"Benefits\" = '[]' WHERE \"Benefits\" = '' OR \"Benefits\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không cần rollback vì đây là data fix
        }
    }
}
