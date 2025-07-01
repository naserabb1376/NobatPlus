using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class sixmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Citys_CityID",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Citys",
                table: "Citys");

            migrationBuilder.RenameTable(
                name: "Citys",
                newName: "Cities");

            migrationBuilder.AlterColumn<long>(
                name: "CityParentID",
                table: "Cities",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cities",
                table: "Cities",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Cities_CityID",
                table: "Addresses",
                column: "CityID",
                principalTable: "Cities",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cities_CityID",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cities",
                table: "Cities");

            migrationBuilder.RenameTable(
                name: "Cities",
                newName: "Citys");

            migrationBuilder.AlterColumn<int>(
                name: "CityParentID",
                table: "Citys",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

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
    }
}
