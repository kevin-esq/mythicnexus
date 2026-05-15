using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MythicNexus.Api.Http;
using MythicNexus.Application.Authorization;
using MythicNexus.Application.Errors;
using MythicNexus.Domain.Entities;
using MythicNexus.Infrastructure.Persistence;

namespace MythicNexus.Api.Modules.Characters;

public static class CharactersEndpoints
{
    public static WebApplication MapCharactersEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/campaigns/{campaignId:guid}/characters").WithTags("Characters").RequireAuthorization();
        g.MapGet("/", ListCharactersAsync);
        g.MapPost("/", CreateCharacterAsync);

        var one = app.MapGroup("/api/characters").WithTags("Characters").RequireAuthorization();
        one.MapGet("/{id:guid}", GetCharacterAsync);
        one.MapPatch("/{id:guid}", PatchCharacterAsync);
        one.MapDelete("/{id:guid}", DeleteCharacterAsync);

        return app;
    }

    private static (int page, int pageSize) ParsePagination(HttpRequest req)
    {
        var page = int.TryParse(req.Query["page"], out var p) ? Math.Max(1, p) : 1;
        var pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? Math.Clamp(ps, 1, 100) : 20;
        return (page, pageSize);
    }

    private static async Task<IResult> ListCharactersAsync(
        Guid campaignId,
        ClaimsPrincipal claims,
        HttpRequest httpRequest,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        var campaign = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanViewCampaignAsync(userId, campaignId, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        var (page, pageSize) = ParsePagination(httpRequest);
        var q = db.Characters.AsNoTracking()
            .Where(ch => ch.CampaignId == campaignId && ch.DeletedAt == null);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderBy(ch => ch.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ch => new CharacterListItemDto(
                ch.Id,
                ch.Name,
                ch.OwnerUserId,
                ch.Level,
                ch.Race,
                ch.Class,
                ch.CreatedAt,
                ch.UpdatedAt))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return ApiResults.OkData(new PagedResult<CharacterListItemDto>(items, page, pageSize, total, totalPages));
    }

    private static async Task<IResult> CreateCharacterAsync(
        Guid campaignId,
        ClaimsPrincipal claims,
        CreateCharacterRequest body,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        var campaign = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanCreateCharacterAsync(userId, campaignId, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        var name = body.Name?.Trim() ?? string.Empty;
        if (name.Length is < 1 or > 200)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.ValidationFailed,
                "Validation failed",
                StatusCodes.Status400BadRequest,
                detail: "Name is required (max 200 characters).");
        }

        var level = body.Level is >= 1 and <= 40 ? body.Level!.Value : 1;
        var now = DateTimeOffset.UtcNow;
        var character = new Character
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            OwnerUserId = userId,
            Name = name,
            Race = string.IsNullOrWhiteSpace(body.Race) ? null : body.Race.Trim(),
            Class = string.IsNullOrWhiteSpace(body.Class) ? null : body.Class.Trim(),
            Level = level,
            Backstory = string.IsNullOrWhiteSpace(body.Backstory) ? null : body.Backstory.Trim(),
            Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            UpdatedByUserId = userId,
            DeletedAt = null,
        };

        db.Characters.Add(character);
        await db.SaveChangesAsync(cancellationToken);

        return ApiResults.OkData(
            new CharacterDetailDto(
                character.Id,
                character.CampaignId,
                character.OwnerUserId,
                character.Name,
                character.Level,
                character.Race,
                character.Class,
                character.Backstory,
                character.Notes,
                character.CreatedAt,
                character.UpdatedAt));
    }

    private static async Task<IResult> GetCharacterAsync(
        Guid id,
        ClaimsPrincipal claims,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        var ch = await db.Characters.AsNoTracking()
            .Join(
                db.Campaigns.AsNoTracking(),
                c => c.CampaignId,
                c => c.Id,
                (character, campaign) => new { character, campaign })
            .FirstOrDefaultAsync(
                x => x.character.Id == id && x.character.DeletedAt == null && x.campaign.DeletedAt == null,
                cancellationToken);

        if (ch is null || ch.campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanViewCampaignAsync(userId, ch.campaign.Id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        var c = ch.character;
        return ApiResults.OkData(
            new CharacterDetailDto(
                c.Id,
                c.CampaignId,
                c.OwnerUserId,
                c.Name,
                c.Level,
                c.Race,
                c.Class,
                c.Backstory,
                c.Notes,
                c.CreatedAt,
                c.UpdatedAt));
    }

    private static async Task<IResult> PatchCharacterAsync(
        Guid id,
        ClaimsPrincipal claims,
        PatchCharacterRequest body,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        var row = await db.Characters
            .Join(
                db.Campaigns,
                c => c.CampaignId,
                c => c.Id,
                (character, campaign) => new { character, campaign })
            .FirstOrDefaultAsync(
                x => x.character.Id == id && x.character.DeletedAt == null && x.campaign.DeletedAt == null,
                cancellationToken);

        if (row is null || row.campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanEditCharacterAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        var character = row.character;
        if (body.Name is { } n)
        {
            var t = n.Trim();
            if (t.Length is < 1 or > 200)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.ValidationFailed,
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    detail: "Invalid name.");
            }

            character.Name = t;
        }

        if (body.Level is { } lv)
        {
            if (lv is < 1 or > 40)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.ValidationFailed,
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    detail: "Level must be between 1 and 40.");
            }

            character.Level = lv;
        }

        if (body.Race is not null)
        {
            character.Race = string.IsNullOrWhiteSpace(body.Race) ? null : body.Race.Trim();
        }

        if (body.Class is not null)
        {
            character.Class = string.IsNullOrWhiteSpace(body.Class) ? null : body.Class.Trim();
        }

        if (body.Backstory is not null)
        {
            character.Backstory = string.IsNullOrWhiteSpace(body.Backstory) ? null : body.Backstory.Trim();
        }

        if (body.Notes is not null)
        {
            character.Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim();
        }

        character.UpdatedAt = DateTimeOffset.UtcNow;
        character.UpdatedByUserId = userId;
        await db.SaveChangesAsync(cancellationToken);

        return ApiResults.OkData(
            new CharacterDetailDto(
                character.Id,
                character.CampaignId,
                character.OwnerUserId,
                character.Name,
                character.Level,
                character.Race,
                character.Class,
                character.Backstory,
                character.Notes,
                character.CreatedAt,
                character.UpdatedAt));
    }

    private static async Task<IResult> DeleteCharacterAsync(
        Guid id,
        ClaimsPrincipal claims,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        var row = await db.Characters
            .Join(
                db.Campaigns,
                c => c.CampaignId,
                c => c.Id,
                (character, campaign) => new { character, campaign })
            .FirstOrDefaultAsync(
                x => x.character.Id == id && x.character.DeletedAt == null && x.campaign.DeletedAt == null,
                cancellationToken);

        if (row is null || row.campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanEditCharacterAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CharacterForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        row.character.DeletedAt = DateTimeOffset.UtcNow;
        row.character.UpdatedAt = DateTimeOffset.UtcNow;
        row.character.UpdatedByUserId = userId;
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private sealed record CreateCharacterRequest(
        string? Name,
        int? Level,
        string? Race,
        string? Class,
        string? Backstory,
        string? Notes);

    private sealed record PatchCharacterRequest(
        string? Name,
        int? Level,
        string? Race,
        string? Class,
        string? Backstory,
        string? Notes);

    private sealed record CharacterListItemDto(
        Guid Id,
        string Name,
        Guid OwnerUserId,
        int Level,
        string? Race,
        string? Class,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record CharacterDetailDto(
        Guid Id,
        Guid CampaignId,
        Guid OwnerUserId,
        string Name,
        int Level,
        string? Race,
        string? Class,
        string? Backstory,
        string? Notes,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
