namespace MythicNexus.Api.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Character> Characters { get; set; } = new List<Character>();
    public ICollection<LoreEntry> LoreEntries { get; set; } = new List<LoreEntry>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<LoreRelation> LoreRelations { get; set; } = new List<LoreRelation>();
}
