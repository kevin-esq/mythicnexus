namespace MythicNexus.Domain.Entities;

public class Character
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    /// <summary>User who owns this character sheet (permissions, private notes, etc.).</summary>
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Race { get; set; }
    public string? Class { get; set; }
    public string? Backstory { get; set; }
    public string? Notes { get; set; }
    public int Level { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
