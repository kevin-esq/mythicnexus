namespace MythicNexus.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public bool EmailConfirmed { get; set; }
    public DateTimeOffset? EmailConfirmedAt { get; set; }

    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastSuccessfulLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    public ICollection<Campaign> OwnedCampaigns { get; set; } = new List<Campaign>();
    public ICollection<LoreEntry> AuthoredLoreEntries { get; set; } = new List<LoreEntry>();

    public ICollection<TenantMembership> TenantMemberships { get; set; } = new List<TenantMembership>();
    public ICollection<CampaignMember> CampaignMembers { get; set; } = new List<CampaignMember>();
    public ICollection<Character> OwnedCharacters { get; set; } = new List<Character>();
}
