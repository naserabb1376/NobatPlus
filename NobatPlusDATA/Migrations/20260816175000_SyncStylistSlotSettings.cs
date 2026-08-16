using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NobatPlusDATA.DataLayer;

#nullable disable

namespace NobatPlusDATA.Migrations
{
    [DbContext(typeof(NobatPlusContext))]
    [Migration("20260816175000_SyncStylistSlotSettings")]
    public partial class SyncStylistSlotSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[Stylists]', N'SlotIntervalMinutes') IS NULL
BEGIN
    ALTER TABLE [dbo].[Stylists] ADD [SlotIntervalMinutes] int NOT NULL CONSTRAINT [DF_Stylists_SlotIntervalMinutes] DEFAULT (30);
END

IF COL_LENGTH(N'[dbo].[Stylists]', N'BookingCreationMode') IS NULL
BEGIN
    ALTER TABLE [dbo].[Stylists] ADD [BookingCreationMode] nvarchar(max) NOT NULL CONSTRAINT [DF_Stylists_BookingCreationMode] DEFAULT (N'automatic');
END

IF COL_LENGTH(N'[dbo].[Stylists]', N'SlotDisplayMode') IS NULL
BEGIN
    ALTER TABLE [dbo].[Stylists] ADD [SlotDisplayMode] nvarchar(max) NOT NULL CONSTRAINT [DF_Stylists_SlotDisplayMode] DEFAULT (N'all');
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[Stylists]', N'SlotDisplayMode') IS NOT NULL
BEGIN
    DECLARE @SlotDisplayModeDefaultName sysname;
    SELECT @SlotDisplayModeDefaultName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'Stylists' AND c.name = N'SlotDisplayMode';

    IF @SlotDisplayModeDefaultName IS NOT NULL
        EXEC(N'ALTER TABLE [dbo].[Stylists] DROP CONSTRAINT [' + @SlotDisplayModeDefaultName + N']');

    ALTER TABLE [dbo].[Stylists] DROP COLUMN [SlotDisplayMode];
END

IF COL_LENGTH(N'[dbo].[Stylists]', N'BookingCreationMode') IS NOT NULL
BEGIN
    DECLARE @BookingCreationModeDefaultName sysname;
    SELECT @BookingCreationModeDefaultName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'Stylists' AND c.name = N'BookingCreationMode';

    IF @BookingCreationModeDefaultName IS NOT NULL
        EXEC(N'ALTER TABLE [dbo].[Stylists] DROP CONSTRAINT [' + @BookingCreationModeDefaultName + N']');

    ALTER TABLE [dbo].[Stylists] DROP COLUMN [BookingCreationMode];
END

IF COL_LENGTH(N'[dbo].[Stylists]', N'SlotIntervalMinutes') IS NOT NULL
BEGIN
    DECLARE @SlotIntervalMinutesDefaultName sysname;
    SELECT @SlotIntervalMinutesDefaultName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'Stylists' AND c.name = N'SlotIntervalMinutes';

    IF @SlotIntervalMinutesDefaultName IS NOT NULL
        EXEC(N'ALTER TABLE [dbo].[Stylists] DROP CONSTRAINT [' + @SlotIntervalMinutesDefaultName + N']');

    ALTER TABLE [dbo].[Stylists] DROP COLUMN [SlotIntervalMinutes];
END
");
        }
    }
}
