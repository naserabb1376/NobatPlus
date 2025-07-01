using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class fixAddressInUser1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Addresses_AddressID",
                table: "Persons");

            migrationBuilder.AlterColumn<long>(
                name: "AddressID",
                table: "Persons",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            //migrationBuilder.CreateTable(
            //    name: "ApiGuides",
            //    columns: table => new
            //    {
            //        ID = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ApiName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        GuideType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FieldEnglishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FieldDataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FieldFarsiName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FieldRecomendedInputType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ApiGuides", x => x.ID);
            //    });

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Addresses_AddressID",
                table: "Persons",
                column: "AddressID",
                principalTable: "Addresses",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Addresses_AddressID",
                table: "Persons");

            migrationBuilder.DropTable(
                name: "ApiGuides");

            migrationBuilder.AlterColumn<long>(
                name: "AddressID",
                table: "Persons",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Addresses_AddressID",
                table: "Persons",
                column: "AddressID",
                principalTable: "Addresses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
