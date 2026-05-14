using System.Security.Claims;
using MythicNexus.Api.Http;
using MythicNexus.Application.Errors;
using MythicNexus.Application.Users.Contracts;

namespace MythicNexus.Api.Modules.Tenants;

public static class TenantsEndpoints
{
    public static WebApplication MapTenantsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants").WithTags("Tenants");

        group.MapGet("/current", GetCurrentAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetCurrentAsync(
        ClaimsPrincipal principal,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Invalid credentials",
                StatusCodes.Status401Unauthorized);
        }

        var tenant = await auth.GetTenantForUserAsync(userId, cancellationToken);
        if (tenant is null)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.UserNotFound,
                "Tenant not found",
                StatusCodes.Status404NotFound);
        }

        return ApiResults.OkData(tenant);
    }
}
