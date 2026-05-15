using System.Security.Claims;

namespace MythicNexus.Api.Http;

public static class AuthExtensions
{
    public static bool TryParseAuth(this ClaimsPrincipal claims, out Guid userId, out Guid tenantId)
    {
        userId = default;
        tenantId = default;
        if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out userId))
        {
            return false;
        }

        return Guid.TryParse(claims.FindFirstValue("tenant_id"), out tenantId);
    }
}
