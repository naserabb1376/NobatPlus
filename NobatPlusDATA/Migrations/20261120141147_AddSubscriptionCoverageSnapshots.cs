using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionCoverageSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxMemberCountSnapshot",
                table: "BusinessSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PlanTypeSnapshot",
                table: "BusinessSubscriptions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Individual");

            // Preserve the contract of subscriptions created before the snapshot columns existed.
            migrationBuilder.Sql(@"
                UPDATE bs
                SET bs.PlanTypeSnapshot = CASE WHEN w.IsWorkShop = 1 THEN 'Business' ELSE 'Individual' END,
                    bs.MaxMemberCountSnapshot = CASE WHEN sp.MaxMemberCount >= 1 THEN sp.MaxMemberCount ELSE 1 END
                FROM BusinessSubscriptions bs
                INNER JOIN Workers w ON w.ID = bs.BusinessID
                LEFT JOIN SubscriptionPlans sp ON sp.ID = bs.SubscriptionPlanID;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BusinessSubscriptions_Snapshot",
                table: "BusinessSubscriptions",
                sql: "[PlanTypeSnapshot] IN ('Individual','Business') AND [MaxMemberCountSnapshot] >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BusinessSubscriptions_Snapshot",
                table: "BusinessSubscriptions");

            migrationBuilder.DropColumn(
                name: "MaxMemberCountSnapshot",
                table: "BusinessSubscriptions");

            migrationBuilder.DropColumn(
                name: "PlanTypeSnapshot",
                table: "BusinessSubscriptions");
        }
    }
}
