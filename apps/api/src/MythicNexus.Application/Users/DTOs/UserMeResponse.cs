namespace MythicNexus.Application.Users.DTOs;

public sealed class UserMeResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
