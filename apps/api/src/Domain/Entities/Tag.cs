namespace MythicNexus.Api.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public ICollection<LoreEntry> LoreEntries { get; set; } = new List<LoreEntry>();
}
