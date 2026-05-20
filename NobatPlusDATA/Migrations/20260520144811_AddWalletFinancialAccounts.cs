using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletFinancialAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "StylistServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StylistServices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ID",
                table: "StylistServices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "StylistServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DiscountID",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedServiceAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PayedAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentFinished",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentLevel",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "PaymentHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "CodeRequired",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountOwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentID = table.Column<long>(type: "bigint", nullable: false),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    ServiceManagementID = table.Column<long>(type: "bigint", nullable: false),
                    StylistServiceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_ServiceManagements_ServiceManagementID",
                        column: x => x.ServiceManagementID,
                        principalTable: "ServiceManagements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<long>(type: "bigint", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Wallets_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementRequests",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialAccountID = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountOwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrackingCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementRequests", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SettlementRequests_FinancialAccounts_FinancialAccountID",
                        column: x => x.FinancialAccountID,
                        principalTable: "FinancialAccounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletID = table.Column<long>(type: "bigint", nullable: false),
                    BookingID = table.Column<long>(type: "bigint", nullable: true),
                    PaymentID = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletID",
                        column: x => x.WalletID,
                        principalTable: "Wallets",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialAccountID = table.Column<long>(type: "bigint", nullable: false),
                    BookingID = table.Column<long>(type: "bigint", nullable: true),
                    PaymentID = table.Column<long>(type: "bigint", nullable: true),
                    SettlementRequestID = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialAccounts_FinancialAccountID",
                        column: x => x.FinancialAccountID,
                        principalTable: "FinancialAccounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_SettlementRequests_SettlementRequestID",
                        column: x => x.SettlementRequestID,
                        principalTable: "SettlementRequests",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_StylistID",
                table: "FinancialAccounts",
                column: "StylistID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BookingID",
                table: "FinancialTransactions",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FinancialAccountID",
                table: "FinancialTransactions",
                column: "FinancialAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_PaymentID",
                table: "FinancialTransactions",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SettlementRequestID",
                table: "FinancialTransactions",
                column: "SettlementRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentID",
                table: "PaymentDetails",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_ServiceManagementID",
                table: "PaymentDetails",
                column: "ServiceManagementID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_StylistID",
                table: "PaymentDetails",
                column: "StylistID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementRequests_FinancialAccountID",
                table: "SettlementRequests",
                column: "FinancialAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CustomerID",
                table: "Wallets",
                column: "CustomerID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_BookingID",
                table: "WalletTransactions",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_PaymentID",
                table: "WalletTransactions",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletID",
                table: "WalletTransactions",
                column: "WalletID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "SettlementRequests");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "StylistServices");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StylistServices");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "StylistServices");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "StylistServices");

            migrationBuilder.DropColumn(
                name: "DiscountID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DiscountedServiceAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PayedAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentFinished",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentLevel",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RemainAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CodeRequired",
                table: "Discounts");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "PaymentHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
