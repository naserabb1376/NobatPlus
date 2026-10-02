using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusTokenDB.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveProfileToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ActiveProfileId",
                table: "RefreshTokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActiveProfileType",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveProfileId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ActiveProfileType",
                table: "RefreshTokens");
        }
    }
}
