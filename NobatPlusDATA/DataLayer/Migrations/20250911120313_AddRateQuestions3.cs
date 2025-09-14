using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddRateQuestions3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingID",
                table: "RateHistories",
                newName: "StylistID");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "RateHistories",
                newName: "RateDate");

            migrationBuilder.AddColumn<long>(
                name: "CustomerID",
                table: "RateHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_RateHistories_CustomerID",
                table: "RateHistories",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_RateHistories_RateQuestionID",
                table: "RateHistories",
                column: "RateQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_RateHistories_StylistID",
                table: "RateHistories",
                column: "StylistID");

            migrationBuilder.AddForeignKey(
                name: "FK_RateHistories_Customers_CustomerID",
                table: "RateHistories",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RateHistories_RateQuestions_RateQuestionID",
                table: "RateHistories",
                column: "RateQuestionID",
                principalTable: "RateQuestions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RateHistories_Stylists_StylistID",
                table: "RateHistories",
                column: "StylistID",
                principalTable: "Stylists",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RateHistories_Customers_CustomerID",
                table: "RateHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_RateHistories_RateQuestions_RateQuestionID",
                table: "RateHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_RateHistories_Stylists_StylistID",
                table: "RateHistories");

            migrationBuilder.DropIndex(
                name: "IX_RateHistories_CustomerID",
                table: "RateHistories");

            migrationBuilder.DropIndex(
                name: "IX_RateHistories_RateQuestionID",
                table: "RateHistories");

            migrationBuilder.DropIndex(
                name: "IX_RateHistories_StylistID",
                table: "RateHistories");

            migrationBuilder.DropColumn(
                name: "CustomerID",
                table: "RateHistories");

            migrationBuilder.RenameColumn(
                name: "StylistID",
                table: "RateHistories",
                newName: "BookingID");

            migrationBuilder.RenameColumn(
                name: "RateDate",
                table: "RateHistories",
                newName: "BookingDate");
        }
    }
}
