namespace MythicNexus.Domain.Entities;

/// <summary>
/// Directed edge between two lore entries (knowledge graph within a campaign).
/// </summary>
public class LoreRelation
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public Guid SourceLoreEntryId { get; set; }
    public LoreEntry Source { get; set; } = null!;
    public Guid TargetLoreEntryId { get; set; }
    public LoreEntry Target { get; set; } = null!;
    public string RelationType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
