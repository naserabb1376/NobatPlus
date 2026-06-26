using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonID = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedAdminPersonID = table.Column<long>(type: "bigint", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Persons_AssignedAdminPersonID",
                        column: x => x.AssignedAdminPersonID,
                        principalTable: "Persons",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SupportTickets_Persons_PersonID",
                        column: x => x.PersonID,
                        principalTable: "Persons",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketMessages",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupportTicketID = table.Column<long>(type: "bigint", nullable: false),
                    SenderPersonID = table.Column<long>(type: "bigint", nullable: false),
                    IsAdminReply = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SupportTicketMessages_Persons_SenderPersonID",
                        column: x => x.SenderPersonID,
                        principalTable: "Persons",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SupportTicketMessages_SupportTickets_SupportTicketID",
                        column: x => x.SupportTicketID,
                        principalTable: "SupportTickets",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketMessages_SenderPersonID",
                table: "SupportTicketMessages",
                column: "SenderPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketMessages_SupportTicketID_CreateDate",
                table: "SupportTicketMessages",
                columns: new[] { "SupportTicketID", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedAdminPersonID",
                table: "SupportTickets",
                column: "AssignedAdminPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_PersonID_LastMessageAt",
                table: "SupportTickets",
                columns: new[] { "PersonID", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_Status_Priority_LastMessageAt",
                table: "SupportTickets",
                columns: new[] { "Status", "Priority", "LastMessageAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportTicketMessages");

            migrationBuilder.DropTable(
                name: "SupportTickets");
        }
    }
}
