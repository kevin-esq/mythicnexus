namespace MythicNexus.Api.Modules.Campaigns;

public static class CampaignsModuleExtensions
{
    public static IServiceCollection AddCampaignsModule(this IServiceCollection services) => services;

    public static WebApplication MapCampaignsEndpoints(this WebApplication app)
    {
        _ = app.MapGroup("/api/campaigns").WithTags("Campaigns");
        return app;
    }
}
