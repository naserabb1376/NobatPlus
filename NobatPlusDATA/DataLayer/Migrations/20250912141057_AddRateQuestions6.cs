using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddRateQuestions6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StylistQuestionRateAvg",
                table: "RateHistories");

            migrationBuilder.DropColumn(
                name: "StylistRateAvg",
                table: "RateHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "StylistQuestionRateAvg",
                table: "RateHistories",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "StylistRateAvg",
                table: "RateHistories",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
