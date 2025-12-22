using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class initDb5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingTime",
                table: "Bookings");

            migrationBuilder.AddColumn<long>(
                name: "StylistID",
                table: "Reviews",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "BookingID",
                table: "RateHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 58L);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_StylistID",
                table: "Reviews",
                column: "StylistID");

            migrationBuilder.CreateIndex(
                name: "IX_RateHistories_BookingID",
                table: "RateHistories",
                column: "BookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_RateHistories_Bookings_BookingID",
                table: "RateHistories",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Stylists_StylistID",
                table: "Reviews",
                column: "StylistID",
                principalTable: "Stylists",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RateHistories_Bookings_BookingID",
                table: "RateHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Stylists_StylistID",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_StylistID",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_RateHistories_BookingID",
                table: "RateHistories");

            migrationBuilder.DropColumn(
                name: "StylistID",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "BookingID",
                table: "RateHistories");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BookingTime",
                table: "Bookings",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
