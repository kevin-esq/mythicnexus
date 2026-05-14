namespace MythicNexus.Application.Users.DTOs;

public sealed class RegisterResult
{
    public bool RequiresEmailVerification { get; init; }

    public string? AccessToken { get; init; }

    public string Message { get; init; } = string.Empty;
}
