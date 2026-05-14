using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MythicNexus.Api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DomainFoundationLoreGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoreEntries_CampaignId",
                table: "LoreEntries");

            migrationBuilder.RenameColumn(
                name: "Markdown",
                table: "LoreEntries",
                newName: "ContentMarkdown");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "LoreEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "LoreEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "LoreEntries",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Backstory",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "Characters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Race",
                table: "Characters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Users"
                SET "Username" = LEFT(
                  regexp_replace(split_part(lower("Email"), '@', 1), '[^a-z0-9._-]', '-', 'g')
                  || '-' || replace(substring("Id"::text, 1, 8), '-', ''),
                  80
                )
                WHERE "Username" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "LoreEntries" le
                SET "CreatedByUserId" = c."OwnerUserId"
                FROM "Campaigns" c
                WHERE le."CampaignId" = c."Id" AND le."CreatedByUserId" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "LoreEntries" le
                SET "CreatedByUserId" = (SELECT u."Id" FROM "Users" u ORDER BY u."CreatedAt" LIMIT 1)
                WHERE le."CreatedByUserId" IS NULL
                  AND EXISTS (SELECT 1 FROM "Users");
                """);

            migrationBuilder.Sql(
                """
                UPDATE "LoreEntries"
                SET "Slug" = replace("Id"::text, '-', '')
                WHERE "Slug" IS NULL OR trim("Slug") = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "LoreEntries",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "LoreEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LoreRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLoreEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLoreEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoreRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoreRelations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoreRelations_LoreEntries_SourceLoreEntryId",
                        column: x => x.SourceLoreEntryId,
                        principalTable: "LoreEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoreRelations_LoreEntries_TargetLoreEntryId",
                        column: x => x.TargetLoreEntryId,
                        principalTable: "LoreEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoreEntries_CampaignId_Slug",
                table: "LoreEntries",
                columns: new[] { "CampaignId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoreEntries_CreatedByUserId",
                table: "LoreEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoreRelations_CampaignId_SourceLoreEntryId_TargetLoreEntryI~",
                table: "LoreRelations",
                columns: new[] { "CampaignId", "SourceLoreEntryId", "TargetLoreEntryId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoreRelations_SourceLoreEntryId",
                table: "LoreRelations",
                column: "SourceLoreEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LoreRelations_TargetLoreEntryId",
                table: "LoreRelations",
                column: "TargetLoreEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoreEntries_Users_CreatedByUserId",
                table: "LoreEntries",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoreEntries_Users_CreatedByUserId",
                table: "LoreEntries");

            migrationBuilder.DropTable(
                name: "LoreRelations");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_LoreEntries_CampaignId_Slug",
                table: "LoreEntries");

            migrationBuilder.DropIndex(
                name: "IX_LoreEntries_CreatedByUserId",
                table: "LoreEntries");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LoreEntries");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "LoreEntries");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "LoreEntries");

            migrationBuilder.DropColumn(
                name: "Backstory",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Race",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "ContentMarkdown",
                table: "LoreEntries",
                newName: "Markdown");

            migrationBuilder.CreateIndex(
                name: "IX_LoreEntries_CampaignId",
                table: "LoreEntries",
                column: "CampaignId");
        }
    }
}
