using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MythicNexus.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SecurityTenancyAuthHardening : Migration
{
    private static readonly Guid LegacyTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_Tenants", x => x.Id));

        migrationBuilder.InsertData(
            table: "Tenants",
            columns: ["Id", "Name", "Slug", "CreatedAt"],
            values: new object[]
            {
                LegacyTenantId,
                "Legacy workspace",
                "legacy",
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            });

        migrationBuilder.AddColumn<int>(
            name: "AccessFailedCount",
            table: "Users",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "EmailConfirmed",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "EmailConfirmedAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LastLoginIp",
            table: "Users",
            type: "character varying(45)",
            maxLength: 45,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastSuccessfulLoginAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LockoutEnd",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "TenantId",
            table: "Users",
            type: "uuid",
            nullable: false,
            defaultValue: LegacyTenantId);

        migrationBuilder.CreateTable(
            name: "EmailVerificationTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailVerificationTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmailVerificationTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LoginAuditEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EmailNormalized = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                Success = table.Column<bool>(type: "boolean", nullable: false),
                FailureReason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_LoginAuditEvents", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PasswordResetTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_PasswordResetTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_Slug",
            table: "Tenants",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_TenantId",
            table: "Users",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailVerificationTokens_TokenHash",
            table: "EmailVerificationTokens",
            column: "TokenHash");

        migrationBuilder.CreateIndex(
            name: "IX_EmailVerificationTokens_UserId",
            table: "EmailVerificationTokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_LoginAuditEvents_OccurredAt",
            table: "LoginAuditEvents",
            column: "OccurredAt");

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetTokens_TokenHash",
            table: "PasswordResetTokens",
            column: "TokenHash");

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetTokens_UserId",
            table: "PasswordResetTokens",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Tenants_TenantId",
            table: "Users",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Tenants_TenantId",
            table: "Users");

        migrationBuilder.DropTable(
            name: "EmailVerificationTokens");

        migrationBuilder.DropTable(
            name: "LoginAuditEvents");

        migrationBuilder.DropTable(
            name: "PasswordResetTokens");

        migrationBuilder.DropIndex(
            name: "IX_Users_TenantId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "AccessFailedCount",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "EmailConfirmed",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "EmailConfirmedAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LastLoginIp",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LastSuccessfulLoginAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LockoutEnd",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "TenantId",
            table: "Users");

        migrationBuilder.DropTable(
            name: "Tenants");
    }
}
