using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MythicNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoreFoundationEnumsAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Excerpt",
                table: "LoreEntries",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LoreEntries",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "LoreEntries",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "CampaignMembers");

            // Normalize legacy lowercase relation labels to enum member names (C# LoreRelationType).
            migrationBuilder.Sql(
                """
                UPDATE "LoreRelations" SET "RelationType" = 'References' WHERE "RelationType" = 'references';
                UPDATE "LoreRelations" SET "RelationType" = 'RelatedTo' WHERE "RelationType" = 'relatedto' OR "RelationType" = 'related_to';
                UPDATE "LoreRelations" SET "RelationType" = 'Contradicts' WHERE "RelationType" = 'contradicts';
                UPDATE "LoreRelations" SET "RelationType" = 'PartOf' WHERE "RelationType" = 'partof' OR "RelationType" = 'part_of';
                UPDATE "LoreRelations" SET "RelationType" = 'LocatedIn' WHERE "RelationType" = 'locatedin' OR "RelationType" = 'located_in';
                UPDATE "LoreRelations" SET "RelationType" = 'EnemyOf' WHERE "RelationType" = 'enemyof' OR "RelationType" = 'enemy_of';
                UPDATE "LoreRelations" SET "RelationType" = 'AllyOf' WHERE "RelationType" = 'allyof' OR "RelationType" = 'ally_of';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Excerpt",
                table: "LoreEntries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LoreEntries");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "LoreEntries");
        }
    }
}
