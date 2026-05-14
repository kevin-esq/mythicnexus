using MythicNexus.Application.Authorization;
using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Tests;

public sealed class CapabilityRulesTests
{
    [Theory]
    [InlineData(TenantRole.Owner, true)]
    [InlineData(TenantRole.Admin, true)]
    [InlineData(TenantRole.Member, false)]
    [InlineData(TenantRole.Viewer, false)]
    public void Tenant_CanManageWorkspace_matches_role(TenantRole role, bool expected) =>
        Assert.Equal(expected, TenantCapabilityRules.CanManageWorkspace(role));

    [Theory]
    [InlineData(TenantRole.Viewer, CampaignRole.DungeonMaster, false)]
    [InlineData(TenantRole.Member, CampaignRole.DungeonMaster, true)]
    [InlineData(TenantRole.Member, CampaignRole.Player, false)]
    [InlineData(TenantRole.Owner, null, true)]
    public void Campaign_delete_requires_tenant_seat_and_campaign_lead_where_relevant(
        TenantRole tenant,
        CampaignRole? campaign,
        bool expected) =>
        Assert.Equal(expected, CampaignCapabilityRules.CanDeleteCampaign(tenant, campaign));

    [Fact]
    public void Player_can_create_character_in_campaign()
    {
        Assert.True(CampaignCapabilityRules.CanCreateCharacter(CampaignRole.Player));
        Assert.False(CampaignCapabilityRules.CanCreateCharacter(CampaignRole.Viewer));
    }
}
