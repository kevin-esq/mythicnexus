using Microsoft.EntityFrameworkCore;
using MythicNexus.Domain.Entities;
using MythicNexus.Infrastructure.Persistence;

namespace MythicNexus.Application.Authorization;

public sealed class TenantPermissionService(MythicNexusDbContext db) : ITenantPermissionService
{
    public async Task<TenantRole?> GetRoleAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var row = await db.TenantMemberships.AsNoTracking()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId, cancellationToken);
        return row?.Role;
    }

    public async Task<bool> CanManageWorkspaceAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, tenantId, cancellationToken);
        return role.HasValue && TenantCapabilityRules.CanManageWorkspace(role.Value);
    }

    public async Task<bool> CanInviteUsersAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, tenantId, cancellationToken);
        return role.HasValue && TenantCapabilityRules.CanInviteUsers(role.Value);
    }

    public async Task<bool> CanManageBillingAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, tenantId, cancellationToken);
        return role.HasValue && TenantCapabilityRules.CanManageBilling(role.Value);
    }

    public async Task<bool> CanManageAllCampaignsInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, tenantId, cancellationToken);
        return role.HasValue && TenantCapabilityRules.CanManageAllCampaignsInTenant(role.Value);
    }

    public async Task<bool> CanCreateCampaignInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(userId, tenantId, cancellationToken);
        return role.HasValue && TenantCapabilityRules.CanCreateCampaignInTenant(role.Value);
    }
}
