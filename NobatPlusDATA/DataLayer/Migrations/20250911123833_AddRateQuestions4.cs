using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddRateQuestions4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "RateScore",
                table: "RateHistories",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddColumn<int>(
                name: "StylistRateCount",
                table: "RateHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StylistQuestionRateAvg",
                table: "RateHistories");

            migrationBuilder.DropColumn(
                name: "StylistRateAvg",
                table: "RateHistories");

            migrationBuilder.DropColumn(
                name: "StylistRateCount",
                table: "RateHistories");

            migrationBuilder.AlterColumn<int>(
                name: "RateScore",
                table: "RateHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
