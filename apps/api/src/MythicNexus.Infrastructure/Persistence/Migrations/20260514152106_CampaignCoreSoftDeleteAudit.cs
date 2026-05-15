using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MythicNexus.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CampaignCoreSoftDeleteAudit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Characters_CampaignId",
            table: "Characters");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DeletedAt",
            table: "LoreEntries",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "UpdatedByUserId",
            table: "LoreEntries",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DeletedAt",
            table: "Characters",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Level",
            table: "Characters",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "UpdatedAt",
            table: "Characters",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "UpdatedByUserId",
            table: "Characters",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CreatedByUserId",
            table: "Campaigns",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DeletedAt",
            table: "Campaigns",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "UpdatedAt",
            table: "Campaigns",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "UpdatedByUserId",
            table: "Campaigns",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE "Characters" SET "UpdatedAt" = "CreatedAt" WHERE "UpdatedAt" IS NULL;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE "Characters" ALTER COLUMN "UpdatedAt" SET NOT NULL;
            """);

        migrationBuilder.Sql(
            """
            UPDATE "Campaigns" SET "CreatedByUserId" = "OwnerUserId", "UpdatedAt" = "CreatedAt"
            WHERE "CreatedByUserId" IS NULL OR "UpdatedAt" IS NULL;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE "Campaigns" ALTER COLUMN "CreatedByUserId" SET NOT NULL;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE "Campaigns" ALTER COLUMN "UpdatedAt" SET NOT NULL;
            """);

        migrationBuilder.Sql(
            """
            UPDATE "LoreEntries" SET "UpdatedByUserId" = "CreatedByUserId" WHERE "UpdatedByUserId" IS NULL;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_LoreEntries_CampaignId_DeletedAt",
            table: "LoreEntries",
            columns: new[] { "CampaignId", "DeletedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_LoreEntries_UpdatedByUserId",
            table: "LoreEntries",
            column: "UpdatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Characters_CampaignId_DeletedAt",
            table: "Characters",
            columns: new[] { "CampaignId", "DeletedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Characters_UpdatedByUserId",
            table: "Characters",
            column: "UpdatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Campaigns_CreatedByUserId",
            table: "Campaigns",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Campaigns_TenantId_DeletedAt",
            table: "Campaigns",
            columns: new[] { "TenantId", "DeletedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Campaigns_UpdatedByUserId",
            table: "Campaigns",
            column: "UpdatedByUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Campaigns_Users_CreatedByUserId",
            table: "Campaigns",
            column: "CreatedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Campaigns_Users_UpdatedByUserId",
            table: "Campaigns",
            column: "UpdatedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Characters_Users_UpdatedByUserId",
            table: "Characters",
            column: "UpdatedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_LoreEntries_Users_UpdatedByUserId",
            table: "LoreEntries",
            column: "UpdatedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Campaigns_Users_CreatedByUserId",
            table: "Campaigns");

        migrationBuilder.DropForeignKey(
            name: "FK_Campaigns_Users_UpdatedByUserId",
            table: "Campaigns");

        migrationBuilder.DropForeignKey(
            name: "FK_Characters_Users_UpdatedByUserId",
            table: "Characters");

        migrationBuilder.DropForeignKey(
            name: "FK_LoreEntries_Users_UpdatedByUserId",
            table: "LoreEntries");

        migrationBuilder.DropIndex(
            name: "IX_LoreEntries_CampaignId_DeletedAt",
            table: "LoreEntries");

        migrationBuilder.DropIndex(
            name: "IX_LoreEntries_UpdatedByUserId",
            table: "LoreEntries");

        migrationBuilder.DropIndex(
            name: "IX_Characters_CampaignId_DeletedAt",
            table: "Characters");

        migrationBuilder.DropIndex(
            name: "IX_Characters_UpdatedByUserId",
            table: "Characters");

        migrationBuilder.DropIndex(
            name: "IX_Campaigns_CreatedByUserId",
            table: "Campaigns");

        migrationBuilder.DropIndex(
            name: "IX_Campaigns_TenantId_DeletedAt",
            table: "Campaigns");

        migrationBuilder.DropIndex(
            name: "IX_Campaigns_UpdatedByUserId",
            table: "Campaigns");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "LoreEntries");

        migrationBuilder.DropColumn(
            name: "UpdatedByUserId",
            table: "LoreEntries");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "Level",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "UpdatedByUserId",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "CreatedByUserId",
            table: "Campaigns");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "Campaigns");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Campaigns");

        migrationBuilder.DropColumn(
            name: "UpdatedByUserId",
            table: "Campaigns");

        migrationBuilder.CreateIndex(
            name: "IX_Characters_CampaignId",
            table: "Characters",
            column: "CampaignId");
    }
}
