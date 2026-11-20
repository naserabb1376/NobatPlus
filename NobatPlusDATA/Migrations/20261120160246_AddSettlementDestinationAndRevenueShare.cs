using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementDestinationAndRevenueShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BusinessBillingSettings_Values",
                table: "BusinessBillingSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "RevenueSharePercent",
                table: "WorkerBillingSettings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SettlementDestination",
                table: "BusinessBillingSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Business");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkerBillingSettings_RevenueSharePercent",
                table: "WorkerBillingSettings",
                sql: "[RevenueSharePercent] >= 0 AND [RevenueSharePercent] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BusinessBillingSettings_Values",
                table: "BusinessBillingSettings",
                sql: "[DefaultFeePayer] IN ('Business','Worker') AND [CollectionMode] IN ('Prepaid','Postpaid','Hybrid') AND [InvoiceCycle] IN ('Weekly','Monthly') AND [SettlementDestination] IN ('Business','Separate') AND [CreditLimit] >= 0 AND [DueDays] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkerBillingSettings_RevenueSharePercent",
                table: "WorkerBillingSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BusinessBillingSettings_Values",
                table: "BusinessBillingSettings");

            migrationBuilder.DropColumn(
                name: "RevenueSharePercent",
                table: "WorkerBillingSettings");

            migrationBuilder.DropColumn(
                name: "SettlementDestination",
                table: "BusinessBillingSettings");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BusinessBillingSettings_Values",
                table: "BusinessBillingSettings",
                sql: "[DefaultFeePayer] IN ('Business','Worker') AND [CollectionMode] IN ('Prepaid','Postpaid','Hybrid') AND [InvoiceCycle] IN ('Weekly','Monthly') AND [CreditLimit] >= 0 AND [DueDays] >= 0");
        }
    }
}
