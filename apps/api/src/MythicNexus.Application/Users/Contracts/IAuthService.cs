using MythicNexus.Application.Users.DTOs;

namespace MythicNexus.Application.Users.Contracts;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserMeResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);

    Task RequestResendVerificationAsync(string email, CancellationToken cancellationToken = default);

    Task<TenantSummaryResponse?> GetTenantForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
