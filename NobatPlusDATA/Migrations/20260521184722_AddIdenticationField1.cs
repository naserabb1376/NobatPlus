using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddIdenticationField1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentificationCode",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileNumber",
                table: "Images",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileNumber",
                table: "FileUploads",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentificationCode",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FileNumber",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "FileNumber",
                table: "FileUploads");
        }
    }
}
