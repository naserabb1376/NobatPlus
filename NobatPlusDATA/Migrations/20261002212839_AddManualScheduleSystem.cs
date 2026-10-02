using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddManualScheduleSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey",
                table: "StylistServicePriceVariants");

            migrationBuilder.AddColumn<long>(
                name: "BookingTagID",
                table: "StylistServicePriceVariants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepositPercentSnapshot",
                table: "BookingServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentSnapshot",
                table: "BookingServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutesSnapshot",
                table: "BookingServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAfterDiscountSnapshot",
                table: "BookingServices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StylistServicePriceVariantID",
                table: "BookingServices",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceSnapshot",
                table: "BookingServices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ScheduleBlockID",
                table: "Bookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingTags",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTags", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookingTags_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StylistScheduleBlocks",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceManagementID = table.Column<long>(type: "bigint", nullable: true),
                    StylistServicePriceVariantID = table.Column<long>(type: "bigint", nullable: true),
                    BookingTagID = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceOverride = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DepositPercentOverride = table.Column<int>(type: "int", nullable: true),
                    IsBookable = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StylistScheduleBlocks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StylistScheduleBlocks_BookingTags_BookingTagID",
                        column: x => x.BookingTagID,
                        principalTable: "BookingTags",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_StylistScheduleBlocks_ServiceManagements_ServiceManagementID",
                        column: x => x.ServiceManagementID,
                        principalTable: "ServiceManagements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_StylistScheduleBlocks_StylistServicePriceVariants_StylistServicePriceVariantID",
                        column: x => x.StylistServicePriceVariantID,
                        principalTable: "StylistServicePriceVariants",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_StylistScheduleBlocks_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StylistServicePriceVariants_BookingTagID",
                table: "StylistServicePriceVariants",
                column: "BookingTagID");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey",
                table: "StylistServicePriceVariants",
                columns: new[] { "StylistID", "ServiceManagementID", "OptionValueCombinationKey" },
                unique: true,
                filter: "[BookingTagID] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey_BookingTagID",
                table: "StylistServicePriceVariants",
                columns: new[] { "StylistID", "ServiceManagementID", "OptionValueCombinationKey", "BookingTagID" },
                unique: true,
                filter: "[BookingTagID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServices_StylistServicePriceVariantID",
                table: "BookingServices",
                column: "StylistServicePriceVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ScheduleBlockID",
                table: "Bookings",
                column: "ScheduleBlockID",
                filter: "[ScheduleBlockID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTags_StylistID_Title",
                table: "BookingTags",
                columns: new[] { "StylistID", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StylistScheduleBlocks_BookingTagID",
                table: "StylistScheduleBlocks",
                column: "BookingTagID");

            migrationBuilder.CreateIndex(
                name: "IX_StylistScheduleBlocks_ServiceManagementID",
                table: "StylistScheduleBlocks",
                column: "ServiceManagementID");

            migrationBuilder.CreateIndex(
                name: "IX_StylistScheduleBlocks_StylistID_StartDateTime_EndDateTime",
                table: "StylistScheduleBlocks",
                columns: new[] { "StylistID", "StartDateTime", "EndDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_StylistScheduleBlocks_StylistServicePriceVariantID",
                table: "StylistScheduleBlocks",
                column: "StylistServicePriceVariantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_StylistScheduleBlocks_ScheduleBlockID",
                table: "Bookings",
                column: "ScheduleBlockID",
                principalTable: "StylistScheduleBlocks",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingServices_StylistServicePriceVariants_StylistServicePriceVariantID",
                table: "BookingServices",
                column: "StylistServicePriceVariantID",
                principalTable: "StylistServicePriceVariants",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_StylistServicePriceVariants_BookingTags_BookingTagID",
                table: "StylistServicePriceVariants",
                column: "BookingTagID",
                principalTable: "BookingTags",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_StylistScheduleBlocks_ScheduleBlockID",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingServices_StylistServicePriceVariants_StylistServicePriceVariantID",
                table: "BookingServices");

            migrationBuilder.DropForeignKey(
                name: "FK_StylistServicePriceVariants_BookingTags_BookingTagID",
                table: "StylistServicePriceVariants");

            migrationBuilder.DropTable(
                name: "StylistScheduleBlocks");

            migrationBuilder.DropTable(
                name: "BookingTags");

            migrationBuilder.DropIndex(
                name: "IX_StylistServicePriceVariants_BookingTagID",
                table: "StylistServicePriceVariants");

            migrationBuilder.DropIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey",
                table: "StylistServicePriceVariants");

            migrationBuilder.DropIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey_BookingTagID",
                table: "StylistServicePriceVariants");

            migrationBuilder.DropIndex(
                name: "IX_BookingServices_StylistServicePriceVariantID",
                table: "BookingServices");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ScheduleBlockID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingTagID",
                table: "StylistServicePriceVariants");

            migrationBuilder.DropColumn(
                name: "DepositPercentSnapshot",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "DiscountPercentSnapshot",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "DurationMinutesSnapshot",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "PriceAfterDiscountSnapshot",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "StylistServicePriceVariantID",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "UnitPriceSnapshot",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "ScheduleBlockID",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey",
                table: "StylistServicePriceVariants",
                columns: new[] { "StylistID", "ServiceManagementID", "OptionValueCombinationKey" },
                unique: true);
        }
    }
}
