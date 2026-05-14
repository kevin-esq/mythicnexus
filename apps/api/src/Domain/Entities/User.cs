namespace MythicNexus.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Campaign> OwnedCampaigns { get; set; } = new List<Campaign>();
    public ICollection<LoreEntry> AuthoredLoreEntries { get; set; } = new List<LoreEntry>();
}
