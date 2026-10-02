using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonStylistProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM [Stylists]
    GROUP BY [PersonID], [IsWorkShop]
    HAVING COUNT(*) > 1
)
BEGIN
    THROW 51000, N'برای یک یا چند کاربر بیش از یک پروفایل هم‌نوع سالن یا آرایشگر وجود دارد. قبل از اجرای Migration داده‌های تکراری Stylists را اصلاح کنید.', 1;
END;");

            migrationBuilder.DropIndex(
                name: "IX_Stylists_PersonID",
                table: "Stylists");

            migrationBuilder.CreateIndex(
                name: "IX_Stylists_PersonID_IsWorkShop",
                table: "Stylists",
                columns: new[] { "PersonID", "IsWorkShop" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stylists_PersonID_IsWorkShop",
                table: "Stylists");

            migrationBuilder.CreateIndex(
                name: "IX_Stylists_PersonID",
                table: "Stylists",
                column: "PersonID");
        }
    }
}
