using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260702000000_RepairPaymentInstallmentColumns")]
    public partial class RepairPaymentInstallmentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'Payments'
                          AND column_name = 'InstallmentNumber'
                    ) THEN
                        ALTER TABLE "Payments"
                        ADD COLUMN "InstallmentNumber" integer NOT NULL DEFAULT 1;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'Payments'
                          AND column_name = 'InstallmentPercentage'
                    ) THEN
                        ALTER TABLE "Payments"
                        ADD COLUMN "InstallmentPercentage" numeric(5,2) NOT NULL DEFAULT 100;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'Payments'
                          AND column_name = 'RequiredAmount'
                    ) THEN
                        ALTER TABLE "Payments"
                        ADD COLUMN "RequiredAmount" numeric(18,2) NOT NULL DEFAULT 0;
                    END IF;

                    UPDATE "Payments"
                    SET "RequiredAmount" = "TotalAmount"
                    WHERE "RequiredAmount" = 0;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep these columns because the current model requires them.
        }
    }
}
