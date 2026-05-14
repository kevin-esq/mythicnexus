namespace MythicNexus.Domain.Entities;

/// <summary>
/// Participation role inside a single campaign (RPG layer). Independent of <see cref="TenantRole"/>.
/// </summary>
public enum CampaignRole
{
    DungeonMaster = 0,
    CoDungeonMaster = 1,
    Player = 2,
    Viewer = 3,
}
