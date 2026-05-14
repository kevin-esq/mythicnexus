namespace MythicNexus.Api.Modules.Lore;

public static class LoreModuleExtensions
{
    public static IServiceCollection AddLoreModule(this IServiceCollection services) => services;

    public static WebApplication MapLoreEndpoints(this WebApplication app)
    {
        _ = app.MapGroup("/api/lore").WithTags("Lore");
        return app;
    }
}
