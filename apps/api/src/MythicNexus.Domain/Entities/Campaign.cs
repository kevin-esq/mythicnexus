namespace MythicNexus.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }

    /// <summary>Workspace this campaign belongs to (organizational boundary for tenant RBAC).</summary>
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Character> Characters { get; set; } = new List<Character>();
    public ICollection<LoreEntry> LoreEntries { get; set; } = new List<LoreEntry>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<LoreRelation> LoreRelations { get; set; } = new List<LoreRelation>();
    public ICollection<CampaignMember> Members { get; set; } = new List<CampaignMember>();
}
