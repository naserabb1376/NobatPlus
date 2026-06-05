using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NobatPlusDATA.DataLayer;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    [DbContext(typeof(NobatPlusContext))]
    [Migration("20260606110000_AddCombinationKeyToStylistServicePriceVariants")]
    public partial class AddCombinationKeyToStylistServicePriceVariants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionValueCombinationKey",
                table: "StylistServicePriceVariants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE v
SET OptionValueCombinationKey = ISNULL(k.CombinationKey, '')
FROM StylistServicePriceVariants v
OUTER APPLY
(
    SELECT STRING_AGG(CAST(ov.ServiceOptionValueID AS nvarchar(20)), '|')
           WITHIN GROUP (ORDER BY ov.ServiceOptionValueID) AS CombinationKey
    FROM StylistServicePriceVariantOptionValues ov
    WHERE ov.StylistServicePriceVariantID = v.ID
) k;
");

            migrationBuilder.CreateIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey",
                table: "StylistServicePriceVariants",
                columns: new[] { "StylistID", "ServiceManagementID", "OptionValueCombinationKey" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StylistServicePriceVariants_StylistID_ServiceManagementID_OptionValueCombinationKey",
                table: "StylistServicePriceVariants");

            migrationBuilder.DropColumn(
                name: "OptionValueCombinationKey",
                table: "StylistServicePriceVariants");
        }
    }
}
