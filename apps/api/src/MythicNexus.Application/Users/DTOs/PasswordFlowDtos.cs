namespace MythicNexus.Application.Users.DTOs;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class ResendVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}
