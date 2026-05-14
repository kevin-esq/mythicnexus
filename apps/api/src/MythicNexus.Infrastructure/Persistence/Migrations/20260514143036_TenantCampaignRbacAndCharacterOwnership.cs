using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MythicNexus.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class TenantCampaignRbacAndCharacterOwnership : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CampaignMembers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CampaignMembers", x => x.Id);
                table.ForeignKey(
                    name: "FK_CampaignMembers_Campaigns_CampaignId",
                    column: x => x.CampaignId,
                    principalTable: "Campaigns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CampaignMembers_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TenantMemberships",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TenantMemberships", x => x.Id);
                table.ForeignKey(
                    name: "FK_TenantMemberships_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TenantMemberships_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CampaignMembers_CampaignId_UserId",
            table: "CampaignMembers",
            columns: new[] { "CampaignId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CampaignMembers_UserId",
            table: "CampaignMembers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_TenantMemberships_TenantId_UserId",
            table: "TenantMemberships",
            columns: new[] { "TenantId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TenantMemberships_UserId",
            table: "TenantMemberships",
            column: "UserId");

        migrationBuilder.AddColumn<Guid>(
            name: "TenantId",
            table: "Campaigns",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE "Campaigns" AS c
            SET "TenantId" = u."TenantId"
            FROM "Users" AS u
            WHERE u."Id" = c."OwnerUserId";
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE "Campaigns" ALTER COLUMN "TenantId" SET NOT NULL;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Campaigns_TenantId",
            table: "Campaigns",
            column: "TenantId");

        migrationBuilder.AddForeignKey(
            name: "FK_Campaigns_Tenants_TenantId",
            table: "Campaigns",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddColumn<Guid>(
            name: "OwnerUserId",
            table: "Characters",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE "Characters" AS ch
            SET "OwnerUserId" = c."OwnerUserId"
            FROM "Campaigns" AS c
            WHERE c."Id" = ch."CampaignId";
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE "Characters" ALTER COLUMN "OwnerUserId" SET NOT NULL;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Characters_OwnerUserId",
            table: "Characters",
            column: "OwnerUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Characters_Users_OwnerUserId",
            table: "Characters",
            column: "OwnerUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        // TenantRole.Owner = 0, CampaignRole.DungeonMaster = 0
        migrationBuilder.Sql(
            """
            INSERT INTO "TenantMemberships" ("Id", "TenantId", "UserId", "Role", "CreatedAt")
            SELECT gen_random_uuid(), u."TenantId", u."Id", 0, NOW()
            FROM "Users" AS u
            WHERE NOT EXISTS (
                SELECT 1 FROM "TenantMemberships" AS m
                WHERE m."TenantId" = u."TenantId" AND m."UserId" = u."Id");
            """);

        migrationBuilder.Sql(
            """
            INSERT INTO "CampaignMembers" ("Id", "CampaignId", "UserId", "Role", "JoinedAt")
            SELECT gen_random_uuid(), c."Id", c."OwnerUserId", 0, c."CreatedAt"
            FROM "Campaigns" AS c
            WHERE NOT EXISTS (
                SELECT 1 FROM "CampaignMembers" AS m
                WHERE m."CampaignId" = c."Id" AND m."UserId" = c."OwnerUserId");
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Characters_Users_OwnerUserId",
            table: "Characters");

        migrationBuilder.DropIndex(
            name: "IX_Characters_OwnerUserId",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "OwnerUserId",
            table: "Characters");

        migrationBuilder.DropForeignKey(
            name: "FK_Campaigns_Tenants_TenantId",
            table: "Campaigns");

        migrationBuilder.DropIndex(
            name: "IX_Campaigns_TenantId",
            table: "Campaigns");

        migrationBuilder.DropColumn(
            name: "TenantId",
            table: "Campaigns");

        migrationBuilder.DropTable(
            name: "CampaignMembers");

        migrationBuilder.DropTable(
            name: "TenantMemberships");
    }
}
