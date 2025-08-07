using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class addsentstatustoSMSMessageTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SMSMessages_Persons_PersonID",
                table: "SMSMessages");

            migrationBuilder.DropIndex(
                name: "IX_SMSMessages_PersonID",
                table: "SMSMessages");

            migrationBuilder.DropColumn(
                name: "PersonID",
                table: "SMSMessages");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "SMSMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "SMSMessages");

            migrationBuilder.AddColumn<long>(
                name: "PersonID",
                table: "SMSMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_SMSMessages_PersonID",
                table: "SMSMessages",
                column: "PersonID");

            migrationBuilder.AddForeignKey(
                name: "FK_SMSMessages_Persons_PersonID",
                table: "SMSMessages",
                column: "PersonID",
                principalTable: "Persons",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
