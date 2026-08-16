using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSchedulingSnapshotsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StylistID",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "RestTimeMinutesSnapshot",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceDurationMinutesSnapshot",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE booking
                SET
                    booking.ServiceDurationMinutesSnapshot = durations.TotalMinutes,
                    booking.RestTimeMinutesSnapshot = CASE
                        WHEN stylist.RestTime IS NULL THEN 0
                        ELSE DATEDIFF(MINUTE, CAST('00:00:00' AS time), stylist.RestTime)
                    END
                FROM Bookings AS booking
                INNER JOIN Stylists AS stylist ON stylist.ID = booking.StylistID
                OUTER APPLY
                (
                    SELECT SUM(DATEDIFF(MINUTE, CAST('00:00:00' AS time), stylistService.ServiceDuration)) AS TotalMinutes
                    FROM BookingServices AS bookingService
                    INNER JOIN StylistServices AS stylistService
                        ON stylistService.StylistID = booking.StylistID
                        AND stylistService.ServiceManagementID = bookingService.ServiceManagementID
                    WHERE bookingService.BookingID = booking.ID
                ) AS durations;");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerID_BookingDate_IsCancelled",
                table: "Bookings",
                columns: new[] { "CustomerID", "BookingDate", "IsCancelled" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StylistID_BookingDate_IsCancelled",
                table: "Bookings",
                columns: new[] { "StylistID", "BookingDate", "IsCancelled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerID_BookingDate_IsCancelled",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StylistID_BookingDate_IsCancelled",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RestTimeMinutesSnapshot",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceDurationMinutesSnapshot",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerID",
                table: "Bookings",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StylistID",
                table: "Bookings",
                column: "StylistID");
        }
    }
}
