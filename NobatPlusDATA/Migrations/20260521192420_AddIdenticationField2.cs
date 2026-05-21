using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddIdenticationField2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
