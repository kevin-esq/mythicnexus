using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MythicNexus.Application.Authorization;
using MythicNexus.Application.Users;
using MythicNexus.Application.Users.Contracts;
using MythicNexus.Application.Users.Services;
using MythicNexus.Application.Validation;

namespace MythicNexus.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.SecretKey) && o.SecretKey.Length >= 32,
                "Jwt:SecretKey must be configured and at least 32 characters.")
            .ValidateOnStart();

        services.AddOptions<AccountLockoutOptions>()
            .Bind(configuration.GetSection(AccountLockoutOptions.SectionName))
            .Validate(o => o.MaxFailedAccessAttempts > 0 && o.LockoutMinutes > 0, "Auth:Lockout must be positive.")
            .ValidateOnStart();

        services.AddOptions<AuthPublicUrlsOptions>()
            .Bind(configuration.GetSection(AuthPublicUrlsOptions.SectionName));

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITenantPermissionService, TenantPermissionService>();
        services.AddScoped<ICampaignPermissionService, CampaignPermissionService>();

        services.AddValidatorsFromAssemblyContaining<UserValidator>();
        return services;
    }
}
