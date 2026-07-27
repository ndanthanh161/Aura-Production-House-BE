using Aura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260727000000_AddFreeMembershipWelcomeOffer")]
public partial class AddFreeMembershipWelcomeOffer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "FreeMembershipClaimedAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "HasClaimedFreeMembership",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FreeMembershipClaimedAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "HasClaimedFreeMembership",
            table: "Users");
    }
}
