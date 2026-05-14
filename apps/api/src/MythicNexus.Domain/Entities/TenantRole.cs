namespace MythicNexus.Domain.Entities;

/// <summary>
/// Workspace-level role. Does <b>not</b> grant RPG actions by itself — use <see cref="CampaignRole"/> for that.
/// A <see cref="TenantRole.Viewer"/> can still be a <see cref="CampaignRole.Player"/> in a campaign.
/// </summary>
public enum TenantRole
{
    Owner = 0,
    Admin = 1,
    Member = 2,
    Viewer = 3,
}
