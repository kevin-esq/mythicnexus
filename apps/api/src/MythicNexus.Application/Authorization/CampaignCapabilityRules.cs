using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Authorization;

/// <summary>
/// Pure functions for in-campaign (RPG) capabilities. Composed with <see cref="TenantCapabilityRules"/> at the use-case layer.
/// </summary>
public static class CampaignCapabilityRules
{
    public static bool CanCreateCharacter(CampaignRole role) =>
        role is CampaignRole.DungeonMaster or CampaignRole.CoDungeonMaster or CampaignRole.Player;

    public static bool CanManageSession(CampaignRole role) =>
        role is CampaignRole.DungeonMaster or CampaignRole.CoDungeonMaster;

    /// <summary>Edit arbitrary lore/session prep; not the same as "edit own player notes".</summary>
    public static bool CanEditSharedLore(CampaignRole role) =>
        role is CampaignRole.DungeonMaster or CampaignRole.CoDungeonMaster;

    /// <summary>
    /// Deleting a campaign requires both a sufficient workspace seat and a strong campaign seat
    /// (tenant <see cref="TenantRole.Viewer"/> must not delete even as in-campaign DM).
    /// </summary>
    public static bool CanDeleteCampaign(TenantRole tenantRole, CampaignRole? campaignRole) =>
        TenantCapabilityRules.CanManageAllCampaignsInTenant(tenantRole)
        || (tenantRole is TenantRole.Member && campaignRole is CampaignRole.DungeonMaster or CampaignRole.CoDungeonMaster);
}
