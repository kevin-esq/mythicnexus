namespace MythicNexus.Api.Domain.Entities;

public class Character
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Race { get; set; }
    public string? Class { get; set; }
    public string? Backstory { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
