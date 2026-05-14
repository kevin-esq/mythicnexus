namespace MythicNexus.Api.Modules.Users;

public static class UsersModuleExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services) => services;

    public static WebApplication MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");
        group.MapGet("/", () => Results.Ok(new { message = "Users module scaffold" }));
        return app;
    }
}
