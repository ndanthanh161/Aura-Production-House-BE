using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260804000000_AddFreeMembershipOfferToggle")]
public partial class AddFreeMembershipOfferToggle : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsFreeMembershipOfferEnabled",
            table: "Packages",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql("""
            UPDATE "Packages"
            SET "IsFreeMembershipOfferEnabled" = TRUE
            WHERE LOWER(TRIM("Name")) = 'membership';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsFreeMembershipOfferEnabled",
            table: "Packages");
    }
}
