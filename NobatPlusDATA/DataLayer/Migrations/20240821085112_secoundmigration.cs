using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class secoundmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCity",
                table: "Addresses");

            migrationBuilder.AddColumn<long>(
                name: "CityID",
                table: "Addresses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityParentID = table.Column<int>(type: "int", nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CityID",
                table: "Addresses",
                column: "CityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_City_CityID",
                table: "Addresses",
                column: "CityID",
                principalTable: "City",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_City_CityID",
                table: "Addresses");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CityID",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CityID",
                table: "Addresses");

            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
