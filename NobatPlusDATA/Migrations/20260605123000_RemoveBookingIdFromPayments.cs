using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NobatPlusDATA.DataLayer;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    [DbContext(typeof(NobatPlusContext))]
    [Migration("20260605123000_RemoveBookingIdFromPayments")]
    public partial class RemoveBookingIdFromPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingID",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BookingID",
                table: "Payments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BookingID",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingID",
                table: "Payments",
                column: "BookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingID",
                table: "Payments",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
