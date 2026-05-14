using Microsoft.EntityFrameworkCore;
using MythicNexus.Domain.Entities;
using MythicNexus.Infrastructure.Persistence;

namespace MythicNexus.Application.Authorization;

public sealed class CampaignPermissionService(MythicNexusDbContext db, ITenantPermissionService tenantPermissions)
    : ICampaignPermissionService
{
    public async Task<CampaignRole?> GetRoleAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var row = await db.CampaignMembers.AsNoTracking()
            .FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == userId, cancellationToken);
        if (row is not null)
        {
            return row.Role;
        }

        var isOwner = await db.Campaigns.AsNoTracking()
            .AnyAsync(c => c.Id == campaignId && c.OwnerUserId == userId, cancellationToken);
        return isOwner ? CampaignRole.DungeonMaster : null;
    }

    public async Task<bool> CanCreateCharacterAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, campaignId, cancellationToken);
        return role.HasValue && CampaignCapabilityRules.CanCreateCharacter(role.Value);
    }

    public async Task<bool> CanEditCharacterAsync(Guid userId, Guid characterId, CancellationToken cancellationToken = default)
    {
        var character = await db.Characters.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken);
        if (character is null)
        {
            return false;
        }

        var campaignRole = await GetRoleAsync(userId, character.CampaignId, cancellationToken);
        if (campaignRole is null)
        {
            return false;
        }

        if (character.OwnerUserId == userId)
        {
            return CampaignCapabilityRules.CanCreateCharacter(campaignRole.Value);
        }

        return campaignRole is CampaignRole.DungeonMaster or CampaignRole.CoDungeonMaster;
    }

    public async Task<bool> CanManageSessionAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, campaignId, cancellationToken);
        return role.HasValue && CampaignCapabilityRules.CanManageSession(role.Value);
    }

    public async Task<bool> CanEditSharedLoreAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, campaignId, cancellationToken);
        return role.HasValue && CampaignCapabilityRules.CanEditSharedLore(role.Value);
    }

    public async Task<bool> CanDeleteCampaignAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);
        if (campaign is null)
        {
            return false;
        }

        var tenantRole = await tenantPermissions.GetRoleAsync(userId, campaign.TenantId, cancellationToken);
        if (!tenantRole.HasValue)
        {
            return false;
        }

        var campaignRole = await GetRoleAsync(userId, campaignId, cancellationToken);
        return CampaignCapabilityRules.CanDeleteCampaign(tenantRole.Value, campaignRole);
    }
}
