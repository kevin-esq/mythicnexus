namespace MythicNexus.Domain.Entities;

/// <summary>
/// Links a user to a workspace (tenant) with an organizational role (billing, invites, campaign admin at workspace scope).
/// </summary>
public class TenantMembership
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public TenantRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
