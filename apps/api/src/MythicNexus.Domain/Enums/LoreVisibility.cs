namespace MythicNexus.Domain.Enums;

/// <summary>
/// Disclosure hint for queries and UX. Real access is enforced by authorization policies.
/// </summary>
public enum LoreVisibility
{
    Public = 0,
    CampaignMembers = 1,
    DungeonMastersOnly = 2,
}
