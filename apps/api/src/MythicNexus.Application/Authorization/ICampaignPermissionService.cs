using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Authorization;

public interface ICampaignPermissionService
{
    Task<CampaignRole?> GetRoleAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanCreateCharacterAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanEditCharacterAsync(Guid userId, Guid characterId, CancellationToken cancellationToken = default);

    Task<bool> CanManageSessionAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanEditSharedLoreAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanDeleteCampaignAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanViewCampaignAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanManageCampaignMetadataAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);

    Task<bool> CanInviteMembersAsync(Guid userId, Guid campaignId, CancellationToken cancellationToken = default);
}
