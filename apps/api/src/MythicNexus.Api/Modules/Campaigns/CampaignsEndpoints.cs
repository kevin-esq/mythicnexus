using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MythicNexus.Api.Http;
using MythicNexus.Application.Authorization;
using MythicNexus.Application.Errors;
using MythicNexus.Application.Users;
using MythicNexus.Application.Users.Contracts;
using MythicNexus.Domain.Entities;
using MythicNexus.Infrastructure.Persistence;

namespace MythicNexus.Api.Modules.Campaigns;

public static class CampaignsEndpoints
{
    public static WebApplication MapCampaignsEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/campaigns").WithTags("Campaigns").RequireAuthorization();

        g.MapGet("/", ListCampaignsAsync);
        g.MapPost("/", CreateCampaignAsync);
        g.MapGet("/{id:guid}", GetCampaignAsync);
        g.MapPatch("/{id:guid}", PatchCampaignAsync);
        g.MapDelete("/{id:guid}", DeleteCampaignAsync);

        g.MapGet("/{id:guid}/members", ListMembersAsync);
        g.MapPost("/{id:guid}/members", AddMemberAsync)
            .WithDescription("Invites a workspace user by userId or username; sends a local outbox email to the invitee. In-app notifications are not implemented yet.");
        g.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMemberAsync);

        return app;
    }

    private static (int page, int pageSize) ParsePagination(HttpRequest req)
    {
        var page = int.TryParse(req.Query["page"], out var p) ? Math.Max(1, p) : 1;
        var pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? Math.Clamp(ps, 1, 100) : 20;
        return (page, pageSize);
    }

    private static async Task<IResult> ListCampaignsAsync(
        ClaimsPrincipal claims,
        HttpRequest httpRequest,
        MythicNexusDbContext db,
        ITenantPermissionService tenantPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        if (await tenantPermissions.GetRoleAsync(userId, tenantId, cancellationToken) is null)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden,
                detail: "No tenant membership.");
        }

        var (page, pageSize) = ParsePagination(httpRequest);
        var q = db.Campaigns.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.DeletedAt == null);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CampaignListItemDto(
                c.Id,
                c.Name,
                c.Description,
                c.OwnerUserId,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return ApiResults.OkData(new PagedResult<CampaignListItemDto>(items, page, pageSize, total, totalPages));
    }

    private static async Task<IResult> CreateCampaignAsync(
        ClaimsPrincipal claims,
        CreateCampaignRequest body,
        MythicNexusDbContext db,
        ITenantPermissionService tenantPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var userId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        if (!await tenantPermissions.CanCreateCampaignInTenantAsync(userId, tenantId, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden,
                detail: "Cannot create campaigns in this workspace.");
        }

        var name = body.Name?.Trim() ?? string.Empty;
        if (name.Length is < 2 or > 200)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.ValidationFailed,
                "Validation failed",
                StatusCodes.Status400BadRequest,
                detail: "Name must be between 2 and 200 characters.");
        }

        var now = DateTimeOffset.UtcNow;
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerUserId = userId,
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            DeletedAt = null,
        };

        db.Campaigns.Add(campaign);
        db.CampaignMembers.Add(
            new CampaignMember
            {
                Id = Guid.NewGuid(),
                CampaignId = campaign.Id,
                UserId = userId,
                Role = CampaignRole.DungeonMaster,
                JoinedAt = now,
            });

        await db.SaveChangesAsync(cancellationToken);

        return ApiResults.OkData(new CampaignDetailDto(
            campaign.Id,
            campaign.TenantId,
            campaign.Name,
            campaign.Description,
            campaign.OwnerUserId,
            campaign.CreatedAt,
            campaign.UpdatedAt));
    }

    private static async Task<IResult> GetCampaignAsync(
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

        var campaign = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanViewCampaignAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        return ApiResults.OkData(
            new CampaignDetailDto(
                campaign.Id,
                campaign.TenantId,
                campaign.Name,
                campaign.Description,
                campaign.OwnerUserId,
                campaign.CreatedAt,
                campaign.UpdatedAt));
    }

    private static async Task<IResult> PatchCampaignAsync(
        Guid id,
        ClaimsPrincipal claims,
        PatchCampaignRequest body,
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

        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanManageCampaignMetadataAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        if (body.Name is { } n)
        {
            var t = n.Trim();
            if (t.Length is < 2 or > 200)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.ValidationFailed,
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    detail: "Name must be between 2 and 200 characters.");
            }

            campaign.Name = t;
        }

        if (body.Description is { } d)
        {
            campaign.Description = string.IsNullOrWhiteSpace(d) ? null : d.Trim();
        }

        campaign.UpdatedAt = DateTimeOffset.UtcNow;
        campaign.UpdatedByUserId = userId;
        await db.SaveChangesAsync(cancellationToken);

        return ApiResults.OkData(
            new CampaignDetailDto(
                campaign.Id,
                campaign.TenantId,
                campaign.Name,
                campaign.Description,
                campaign.OwnerUserId,
                campaign.CreatedAt,
                campaign.UpdatedAt));
    }

    private static async Task<IResult> DeleteCampaignAsync(
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

        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanDeleteCampaignAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        campaign.DeletedAt = DateTimeOffset.UtcNow;
        campaign.UpdatedAt = DateTimeOffset.UtcNow;
        campaign.UpdatedByUserId = userId;
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListMembersAsync(
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

        var campaign = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanViewCampaignAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        var joined = await db.CampaignMembers.AsNoTracking()
            .Where(m => m.CampaignId == id)
            .Join(db.Users.AsNoTracking(), m => m.UserId, u => u.Id, (m, u) => new { m, u })
            .OrderBy(x => x.u.Username)
            .Select(x => new
            {
                x.m.UserId,
                x.u.Username,
                x.u.Email,
                x.m.Role,
                x.m.JoinedAt,
            })
            .ToListAsync(cancellationToken);

        var rows = joined
            .Select(x => new CampaignMemberDto(
                x.UserId,
                x.Username,
                x.Email,
                (int)x.Role,
                x.Role.ToString(),
                x.JoinedAt))
            .ToList();

        return ApiResults.OkData(rows);
    }

    private static async Task<IResult> AddMemberAsync(
        Guid id,
        ClaimsPrincipal claims,
        AddCampaignMemberRequest body,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        ITenantPermissionService tenantPermissions,
        IEmailOutbox emailOutbox,
        IOptions<AuthPublicUrlsOptions> publicUrls,
        ILoggerFactory loggerFactory,
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
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanInviteMembersAsync(userId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        Guid inviteeId;
        if (body.UserId is { } uid && uid != Guid.Empty)
        {
            inviteeId = uid;
        }
        else
        {
            var username = body.Username?.Trim() ?? string.Empty;
            if (username.Length == 0)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.ValidationFailed,
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    detail: "Provide userId or username.");
            }

            if (username.Length is < 2 or > 80)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.ValidationFailed,
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    detail: "Username must be between 2 and 80 characters.");
            }

            var unLower = username.ToLowerInvariant();
            var invitee = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == unLower, cancellationToken);
            if (invitee is null)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.UserNotFound,
                    "Not found",
                    StatusCodes.Status404NotFound,
                    detail: "No user with that username.");
            }

            inviteeId = invitee.Id;
        }

        if (await tenantPermissions.GetRoleAsync(inviteeId, tenantId, cancellationToken) is null)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.ValidationFailed,
                "Validation failed",
                StatusCodes.Status400BadRequest,
                detail: "User is not a member of this workspace.");
        }

        if (!Enum.IsDefined(typeof(CampaignRole), body.Role))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.ValidationFailed,
                "Validation failed",
                StatusCodes.Status400BadRequest,
                detail: "Invalid campaign role.");
        }

        if (await db.CampaignMembers.AnyAsync(m => m.CampaignId == id && m.UserId == inviteeId, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.MemberConflict,
                "Conflict",
                StatusCodes.Status409Conflict,
                detail: "User is already a campaign member.");
        }

        var role = (CampaignRole)body.Role;
        var joinedAt = DateTimeOffset.UtcNow;
        db.CampaignMembers.Add(
            new CampaignMember
            {
                Id = Guid.NewGuid(),
                CampaignId = id,
                UserId = inviteeId,
                Role = role,
                JoinedAt = joinedAt,
            });

        await db.SaveChangesAsync(cancellationToken);

        var u = await db.Users.AsNoTracking().FirstAsync(x => x.Id == inviteeId, cancellationToken);
        var inviter = await db.Users.AsNoTracking().FirstAsync(x => x.Id == userId, cancellationToken);
        var webBase = publicUrls.Value.WebBaseUrl.Trim().TrimEnd('/');
        var campaignLink = $"{webBase}/dashboard/campaigns/{id}";
        var roleEs = CampaignRoleLabelEs(role);
        var subject = "Te han añadido a una campaña en MythicNexus";
        var emailBody =
            $"Hola {u.Username},\r\n\r\n" +
            $"{inviter.Username} te ha añadido a la campaña «{campaign.Name}» como {roleEs}.\r\n\r\n" +
            $"Abre la campaña en el panel:\r\n{campaignLink}\r\n\r\n" +
            "(Este mensaje se genera en entorno local en la carpeta de outbox de correo; las notificaciones dentro de la app se añadirán más adelante.)";

        try
        {
            await emailOutbox.WriteMessageAsync(
                $"campaign-invite-{id:N}",
                subject,
                emailBody,
                u.Email,
                cancellationToken);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("Campaigns").LogWarning(
                ex,
                "Failed to write campaign invite email for user {InviteeId} campaign {CampaignId}",
                inviteeId,
                id);
        }

        return ApiResults.OkData(
            new CampaignMemberDto(
                inviteeId,
                u.Username,
                u.Email,
                (int)role,
                role.ToString(),
                joinedAt));
    }

    private static string CampaignRoleLabelEs(CampaignRole role) =>
        role switch
        {
            CampaignRole.DungeonMaster => "maestro de calabozos",
            CampaignRole.CoDungeonMaster => "co-maestro de calabozos",
            CampaignRole.Player => "jugador",
            CampaignRole.Viewer => "espectador",
            _ => role.ToString(),
        };

    private static async Task<IResult> RemoveMemberAsync(
        Guid id,
        Guid userId,
        ClaimsPrincipal claims,
        MythicNexusDbContext db,
        ICampaignPermissionService campaignPermissions,
        CancellationToken cancellationToken)
    {
        if (!claims.TryParseAuth(out var actorId, out var tenantId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Unauthorized",
                StatusCodes.Status401Unauthorized);
        }

        var campaign = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (campaign is null || campaign.TenantId != tenantId)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (!await campaignPermissions.CanInviteMembersAsync(actorId, id, cancellationToken))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.CampaignForbidden,
                "Forbidden",
                StatusCodes.Status403Forbidden);
        }

        var row = await db.CampaignMembers.FirstOrDefaultAsync(m => m.CampaignId == id && m.UserId == userId, cancellationToken);
        if (row is null)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.MemberNotFound,
                "Not found",
                StatusCodes.Status404NotFound);
        }

        if (row.Role == CampaignRole.DungeonMaster)
        {
            var dmCount = await db.CampaignMembers.CountAsync(
                m => m.CampaignId == id && m.Role == CampaignRole.DungeonMaster,
                cancellationToken);
            if (dmCount <= 1)
            {
                return ApiResults.ProblemWithCode(
                    ErrorCodes.ValidationFailed,
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    detail: "Cannot remove the only Dungeon Master.");
            }
        }

        db.CampaignMembers.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private sealed record CreateCampaignRequest(string? Name, string? Description);

    private sealed record PatchCampaignRequest(string? Name, string? Description);

    private sealed record AddCampaignMemberRequest(Guid? UserId, string? Username, int Role);

    private sealed record CampaignListItemDto(
        Guid Id,
        string Name,
        string? Description,
        Guid OwnerUserId,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record CampaignDetailDto(
        Guid Id,
        Guid TenantId,
        string Name,
        string? Description,
        Guid OwnerUserId,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record CampaignMemberDto(
        Guid UserId,
        string Username,
        string Email,
        int Role,
        string RoleName,
        DateTimeOffset JoinedAt);

    private sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
