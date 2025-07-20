using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class modifyStylist1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GenderAccepted",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StylistBio",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StylistName",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkShopInteractMode",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SocialNetworks",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    SocialNetworkName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocialNetworkIcon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialNetworks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SocialNetworks_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTimes",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistID = table.Column<long>(type: "bigint", nullable: false),
                    WorkStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WorkTimes_Stylists_StylistID",
                        column: x => x.StylistID,
                        principalTable: "Stylists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialNetworks_StylistID",
                table: "SocialNetworks",
                column: "StylistID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTimes_StylistID",
                table: "WorkTimes",
                column: "StylistID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialNetworks");

            migrationBuilder.DropTable(
                name: "WorkTimes");

            migrationBuilder.DropColumn(
                name: "GenderAccepted",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "StylistBio",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "StylistName",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "WorkShopInteractMode",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Persons");
        }
    }
}
