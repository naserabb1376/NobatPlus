using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class addServiceGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "ServiceManagements");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ServiceDuration",
                table: "StylistServices",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "ServiceGender",
                table: "ServiceManagements",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceDuration",
                table: "StylistServices");

            migrationBuilder.DropColumn(
                name: "ServiceGender",
                table: "ServiceManagements");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "ServiceManagements",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
