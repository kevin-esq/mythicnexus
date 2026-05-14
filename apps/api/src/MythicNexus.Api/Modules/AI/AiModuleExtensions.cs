namespace MythicNexus.Api.Modules.AI;

public static class AiModuleExtensions
{
    public static IServiceCollection AddAiModule(this IServiceCollection services) => services;

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {
        _ = app.MapGroup("/api/ai").WithTags("AI");
        return app;
    }
}
