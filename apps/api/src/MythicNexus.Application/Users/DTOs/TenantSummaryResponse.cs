namespace MythicNexus.Application.Users.DTOs;

public sealed class TenantSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
