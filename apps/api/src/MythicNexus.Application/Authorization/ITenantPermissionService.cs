using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Authorization;

public interface ITenantPermissionService
{
    Task<TenantRole?> GetRoleAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CanManageWorkspaceAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CanInviteUsersAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CanManageBillingAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CanManageAllCampaignsInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CanCreateCampaignInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
