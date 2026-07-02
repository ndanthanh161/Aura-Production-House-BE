using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallmentNumber",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallmentPercentage",
                table: "Payments",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<decimal>(
                name: "RequiredAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE \"Payments\" SET \"RequiredAmount\" = \"TotalAmount\" WHERE \"RequiredAmount\" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallmentNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InstallmentPercentage",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RequiredAmount",
                table: "Payments");
        }
    }
}
