using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class modifyStylist2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PayMethod",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "WorkShopDepositAmount",
                table: "Stylists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "WorkShopRentAmount",
                table: "Stylists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "PayMethod",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "WorkShopDepositAmount",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "WorkShopRentAmount",
                table: "Stylists");
        }
    }
}
