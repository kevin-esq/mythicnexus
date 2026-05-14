namespace MythicNexus.Domain.Entities;

/// <summary>
/// RPG participation in one campaign. Does not replace <see cref="TenantMembership"/> — both layers apply.
/// </summary>
public class CampaignMember
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public CampaignRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}
