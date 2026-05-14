namespace MythicNexus.Domain.Tenancy;

/// <summary>
/// Pre-seeded tenant for users created before per-workspace tenants existed.
/// </summary>
public static class WellKnownTenantIds
{
    public static readonly Guid LegacyDefault = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
}
