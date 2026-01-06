using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class SettingsTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_Bookings_BookingID",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "PaymentHistories",
                newName: "PaymentID");

            migrationBuilder.AlterColumn<decimal>(
                name: "ServicePrice",
                table: "StylistServices",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<decimal>(
                name: "AllPaymentAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlarformAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StylistAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalServiceAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Settings_Settings_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Settings",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_PaymentID",
                table: "PaymentHistories",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ParentId",
                table: "Settings",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_Bookings_BookingID",
                table: "PaymentHistories",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_Payments_PaymentID",
                table: "PaymentHistories",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_Bookings_BookingID",
                table: "PaymentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_Payments_PaymentID",
                table: "PaymentHistories");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_PaymentID",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "AllPaymentAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlarformAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StylistAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TotalServiceAmount",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "PaymentID",
                table: "PaymentHistories",
                newName: "Amount");

            migrationBuilder.AlterColumn<long>(
                name: "ServicePrice",
                table: "StylistServices",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<long>(
                name: "Amount",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_Bookings_BookingID",
                table: "PaymentHistories",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
