namespace MythicNexus.Application.Users.DTOs;

public enum LoginFailureKind
{
    None = 0,
    InvalidCredentials = 1,
    AccountLocked = 2,
    EmailNotConfirmed = 3,
}

public sealed class LoginResult
{
    public AuthResponse? Auth { get; init; }

    public LoginFailureKind Failure { get; init; }
}
