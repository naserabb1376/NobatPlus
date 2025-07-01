using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class four : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cityes_CityID",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cityes",
                table: "Cityes");

            migrationBuilder.RenameTable(
                name: "Cityes",
                newName: "Citys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Citys",
                table: "Citys",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Citys_CityID",
                table: "Addresses",
                column: "CityID",
                principalTable: "Citys",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Citys_CityID",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Citys",
                table: "Citys");

            migrationBuilder.RenameTable(
                name: "Citys",
                newName: "Cityes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cityes",
                table: "Cityes",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Cityes_CityID",
                table: "Addresses",
                column: "CityID",
                principalTable: "Cityes",
                principalColumn: "ID");
        }
    }
}
