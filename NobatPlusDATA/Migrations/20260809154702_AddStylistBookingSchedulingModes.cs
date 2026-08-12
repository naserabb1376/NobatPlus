using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddStylistBookingSchedulingModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckAvailabilities_StylistID",
                table: "CheckAvailabilities");

            migrationBuilder.AddColumn<string>(
                name: "BookingCreationMode",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "automatic");

            migrationBuilder.AddColumn<string>(
                name: "SlotDisplayMode",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "all");

            migrationBuilder.CreateIndex(
                name: "IX_CheckAvailabilities_StylistID_Date_Time",
                table: "CheckAvailabilities",
                columns: new[] { "StylistID", "Date", "Time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckAvailabilities_StylistID_Date_Time",
                table: "CheckAvailabilities");

            migrationBuilder.DropColumn(
                name: "BookingCreationMode",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "SlotDisplayMode",
                table: "Stylists");

            migrationBuilder.CreateIndex(
                name: "IX_CheckAvailabilities_StylistID",
                table: "CheckAvailabilities",
                column: "StylistID");
        }
    }
}
