using MythicNexus.Domain.Enums;

namespace MythicNexus.Domain.Entities;

public class LoreEntry
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }
    public string ContentMarkdown { get; set; } = string.Empty;
    public LoreStatus Status { get; set; } = LoreStatus.Draft;
    public LoreVisibility Visibility { get; set; } = LoreVisibility.CampaignMembers;
    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<LoreRelation> OutgoingRelations { get; set; } = new List<LoreRelation>();
    public ICollection<LoreRelation> IncomingRelations { get; set; } = new List<LoreRelation>();
}
