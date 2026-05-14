namespace MythicNexus.Api.Modules.Characters;

public static class CharactersModuleExtensions
{
    public static IServiceCollection AddCharactersModule(this IServiceCollection services) => services;

    public static WebApplication MapCharactersEndpoints(this WebApplication app)
    {
        _ = app.MapGroup("/api/characters").WithTags("Characters");
        return app;
    }
}
