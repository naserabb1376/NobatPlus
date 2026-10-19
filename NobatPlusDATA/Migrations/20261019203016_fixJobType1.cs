using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class fixJobType1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStylistOnly",
                table: "Stylists");

            migrationBuilder.AddColumn<long>(
                name: "JobTypeParentID",
                table: "JobTypes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "JobTypes",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_JobTypes_RoleId",
                table: "JobTypes",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTypes_Roles_RoleId",
                table: "JobTypes",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTypes_Roles_RoleId",
                table: "JobTypes");

            migrationBuilder.DropIndex(
                name: "IX_JobTypes_RoleId",
                table: "JobTypes");

            migrationBuilder.DropColumn(
                name: "JobTypeParentID",
                table: "JobTypes");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "JobTypes");

            migrationBuilder.AddColumn<bool>(
                name: "IsStylistOnly",
                table: "Stylists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
