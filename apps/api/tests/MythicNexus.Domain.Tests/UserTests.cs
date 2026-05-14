using MythicNexus.Domain.Entities;

namespace MythicNexus.Domain.Tests;

public sealed class UserTests
{
    [Fact]
    public void User_initializes_navigation_collections()
    {
        var user = new User();

        Assert.NotNull(user.OwnedCampaigns);
        Assert.NotNull(user.AuthoredLoreEntries);
        Assert.NotNull(user.TenantMemberships);
        Assert.NotNull(user.CampaignMembers);
        Assert.NotNull(user.OwnedCharacters);
        Assert.Empty(user.OwnedCampaigns);
        Assert.Empty(user.AuthoredLoreEntries);
        Assert.Empty(user.TenantMemberships);
        Assert.Empty(user.CampaignMembers);
        Assert.Empty(user.OwnedCharacters);
    }
}
