namespace MythicNexus.Application.Users;

public sealed class AuthPublicUrlsOptions
{
    public const string SectionName = "Auth:PublicUrls";

    /// <summary>Public base URL of this API (used in emailed links that hit the API first).</summary>
    public string ApiBaseUrl { get; set; } = "http://localhost:5118";

    /// <summary>Public base URL of the web app (redirects after verify-email, etc.).</summary>
    public string WebBaseUrl { get; set; } = "http://localhost:3000";
}
