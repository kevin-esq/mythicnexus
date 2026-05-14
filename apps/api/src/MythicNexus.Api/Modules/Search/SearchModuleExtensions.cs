namespace MythicNexus.Api.Modules.Search;

public static class SearchModuleExtensions
{
    public static IServiceCollection AddSearchModule(this IServiceCollection services) => services;

    public static WebApplication MapSearchEndpoints(this WebApplication app)
    {
        _ = app.MapGroup("/api/search").WithTags("Search");
        return app;
    }
}
