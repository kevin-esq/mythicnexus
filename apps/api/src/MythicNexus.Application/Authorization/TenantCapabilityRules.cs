using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Authorization;

/// <summary>
/// Pure functions for workspace (tenant) capabilities. Endpoints should call <see cref="ITenantPermissionService"/>
/// instead of duplicating role checks.
/// </summary>
public static class TenantCapabilityRules
{
    public static bool CanManageWorkspace(TenantRole role) =>
        role is TenantRole.Owner or TenantRole.Admin;

    public static bool CanInviteUsers(TenantRole role) =>
        role is TenantRole.Owner or TenantRole.Admin;

    public static bool CanManageBilling(TenantRole role) =>
        role is TenantRole.Owner;

    /// <summary>Org-wide campaign administration (e.g. list/delete any campaign in the tenant).</summary>
    public static bool CanManageAllCampaignsInTenant(TenantRole role) =>
        role is TenantRole.Owner or TenantRole.Admin;
}
