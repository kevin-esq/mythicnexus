using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MythicNexus.Infrastructure.Persistence;

namespace MythicNexus.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<MythicNexusDbContext>(options =>
            options.UseNpgsql(connectionString));
        return services;
    }
}
