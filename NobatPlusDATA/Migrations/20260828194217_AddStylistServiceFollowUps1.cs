using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddStylistServiceFollowUps1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkTimes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WalletTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SupportTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SupportTicketMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Stylists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StylistPacifics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SocialNetworks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SMSMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SettlementRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Settings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServiceManagements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServiceDiscounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Registers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RateQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RateHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PaymentHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PaymentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Logs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Logins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Images",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FinancialTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FileUploads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DiscountAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomerDiscounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Cities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CheckAvailabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ApiGuides",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Admins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AdminAuditLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "StylistServiceFollowUpSettings",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    ServiceManagementID = table.Column<long>(type: "bigint", nullable: false),
                    StylistServicePriceVariantID = table.Column<long>(type: "bigint", nullable: true),
                    RepairEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RepairAfterDays = table.Column<int>(type: "int", nullable: true),
                    RepairReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RepairReminderBeforeDays = table.Column<int>(type: "int", nullable: true),
                    RepairReminderMessageSettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AfterCareEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AfterCareDelayMinutes = table.Column<int>(type: "int", nullable: true),
                    AfterCareMessageSettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StylistServiceFollowUpSettings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StylistServiceFollowUpSettings_ServiceManagements_ServiceManagementID",
                        column: x => x.ServiceManagementID,
                        principalTable: "ServiceManagements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_StylistServiceFollowUpSettings_StylistServicePriceVariants_StylistServicePriceVariantID",
                        column: x => x.StylistServicePriceVariantID,
                        principalTable: "StylistServicePriceVariants",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_StylistServiceFollowUpSettings_StylistServices_StylistID_ServiceManagementID",
                        columns: x => new { x.StylistID, x.ServiceManagementID },
                        principalTable: "StylistServices",
                        principalColumns: new[] { "StylistID", "ServiceManagementID" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StylistServiceFollowUpSettings_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "BookingScheduledMessages",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<long>(type: "bigint", nullable: false),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    CustomerID = table.Column<long>(type: "bigint", nullable: false),
                    ServiceManagementID = table.Column<long>(type: "bigint", nullable: true),
                    StylistServiceFollowUpSettingID = table.Column<long>(type: "bigint", nullable: true),
                    StylistServicePriceVariantID = table.Column<long>(type: "bigint", nullable: true),
                    MessageType = table.Column<byte>(type: "tinyint", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ProviderMessageID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HangfireJobID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SMSMessageID = table.Column<long>(type: "bigint", nullable: true),
                    NotificationID = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingScheduledMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_Notifications_NotificationID",
                        column: x => x.NotificationID,
                        principalTable: "Notifications",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_SMSMessages_SMSMessageID",
                        column: x => x.SMSMessageID,
                        principalTable: "SMSMessages",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_ServiceManagements_ServiceManagementID",
                        column: x => x.ServiceManagementID,
                        principalTable: "ServiceManagements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_StylistServiceFollowUpSettings_StylistServiceFollowUpSettingID",
                        column: x => x.StylistServiceFollowUpSettingID,
                        principalTable: "StylistServiceFollowUpSettings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_StylistServicePriceVariants_StylistServicePriceVariantID",
                        column: x => x.StylistServicePriceVariantID,
                        principalTable: "StylistServicePriceVariants",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_BookingScheduledMessages_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_BookingID_ServiceManagementID_StylistServicePriceVariantID_MessageType_ScheduledAt",
                table: "BookingScheduledMessages",
                columns: new[] { "BookingID", "ServiceManagementID", "StylistServicePriceVariantID", "MessageType", "ScheduledAt" },
                unique: true,
                filter: "[ServiceManagementID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_CustomerID",
                table: "BookingScheduledMessages",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_NotificationID",
                table: "BookingScheduledMessages",
                column: "NotificationID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_ServiceManagementID",
                table: "BookingScheduledMessages",
                column: "ServiceManagementID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_SMSMessageID",
                table: "BookingScheduledMessages",
                column: "SMSMessageID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_Status_ScheduledAt",
                table: "BookingScheduledMessages",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_StylistID",
                table: "BookingScheduledMessages",
                column: "StylistID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_StylistServiceFollowUpSettingID",
                table: "BookingScheduledMessages",
                column: "StylistServiceFollowUpSettingID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingScheduledMessages_StylistServicePriceVariantID",
                table: "BookingScheduledMessages",
                column: "StylistServicePriceVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServiceFollowUpSettings_ServiceManagementID",
                table: "StylistServiceFollowUpSettings",
                column: "ServiceManagementID");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServiceFollowUpSettings_StylistID_ServiceManagementID",
                table: "StylistServiceFollowUpSettings",
                columns: new[] { "StylistID", "ServiceManagementID" });

            migrationBuilder.CreateIndex(
                name: "IX_StylistServiceFollowUpSettings_StylistID_ServiceManagementID_StylistServicePriceVariantID",
                table: "StylistServiceFollowUpSettings",
                columns: new[] { "StylistID", "ServiceManagementID", "StylistServicePriceVariantID" },
                unique: true,
                filter: "[StylistServicePriceVariantID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServiceFollowUpSettings_StylistServicePriceVariantID",
                table: "StylistServiceFollowUpSettings",
                column: "StylistServicePriceVariantID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingScheduledMessages");

            migrationBuilder.DropTable(
                name: "StylistServiceFollowUpSettings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkTimes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SupportTicketMessages");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StylistPacifics");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SocialNetworks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SMSMessages");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SettlementRequests");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServiceManagements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServiceDiscounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Registers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RateQuestions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RateHistories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PaymentDetails");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Logins");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DiscountAssignments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomerDiscounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CheckAvailabilities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ApiGuides");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AdminAuditLogs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Addresses");
        }
    }
}
