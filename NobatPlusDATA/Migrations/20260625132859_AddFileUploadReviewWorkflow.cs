using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUploadReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "FileUploads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "FileUploads",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "FileUploads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReviewedByPersonID",
                table: "FileUploads",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_ReviewedByPersonID",
                table: "FileUploads",
                column: "ReviewedByPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_ReviewStatus_CreateDate",
                table: "FileUploads",
                columns: new[] { "ReviewStatus", "CreateDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_FileUploads_Persons_ReviewedByPersonID",
                table: "FileUploads",
                column: "ReviewedByPersonID",
                principalTable: "Persons",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileUploads_Persons_ReviewedByPersonID",
                table: "FileUploads");

            migrationBuilder.DropIndex(
                name: "IX_FileUploads_ReviewedByPersonID",
                table: "FileUploads");

            migrationBuilder.DropIndex(
                name: "IX_FileUploads_ReviewStatus_CreateDate",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "ReviewedByPersonID",
                table: "FileUploads");

        }
    }
}
