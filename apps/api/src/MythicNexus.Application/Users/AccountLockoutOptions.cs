namespace MythicNexus.Application.Users;

public sealed class AccountLockoutOptions
{
    public const string SectionName = "Auth:Lockout";

    public int MaxFailedAccessAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
