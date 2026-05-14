namespace MythicNexus.Domain.Entities;

/// <summary>
/// Isolation boundary for users and (future) campaign data. New registrations create a dedicated tenant.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<TenantMembership> Memberships { get; set; } = new List<TenantMembership>();
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
