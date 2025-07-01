using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_City_CityID",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_City",
                table: "City");

            migrationBuilder.RenameTable(
                name: "City",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cityes_CityID",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cityes",
                table: "Cityes");

            migrationBuilder.RenameTable(
                name: "Cityes",
                newName: "City");

            migrationBuilder.AddPrimaryKey(
                name: "PK_City",
                table: "City",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_City_CityID",
                table: "Addresses",
                column: "CityID",
                principalTable: "City",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
