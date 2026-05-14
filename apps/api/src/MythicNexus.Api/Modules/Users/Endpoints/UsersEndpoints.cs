using System.Security.Claims;
using FluentValidation;
using Microsoft.Extensions.Options;
using MythicNexus.Api.Http;
using MythicNexus.Application.Errors;
using MythicNexus.Application.Users;
using MythicNexus.Application.Users.Contracts;
using MythicNexus.Application.Users.DTOs;

namespace MythicNexus.Api.Modules.Users.Endpoints;

public static class UsersEndpoints
{
    public static WebApplication MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/register", RegisterAsync).RequireRateLimiting("auth_register");
        group.MapPost("/login", LoginAsync).RequireRateLimiting("auth_login");
        group.MapPost("/forgot-password", ForgotPasswordAsync).RequireRateLimiting("auth_recovery");
        group.MapPost("/reset-password", ResetPasswordAsync).RequireRateLimiting("auth_recovery");
        group.MapPost("/resend-verification", ResendVerificationAsync).RequireRateLimiting("auth_recovery");
        group.MapGet("/verify-email", VerifyEmailAsync).RequireRateLimiting("auth_verify");
        group.MapGet("/me", GetMeAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest dto,
        IValidator<RegisterRequest> validator,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResults.ValidationFailed(validation);
        }

        var response = await auth.RegisterAsync(dto, cancellationToken);
        return ApiResults.OkData(response);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest dto,
        IValidator<LoginRequest> validator,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResults.ValidationFailed(validation);
        }

        var result = await auth.LoginAsync(dto, cancellationToken);
        return result.Failure switch
        {
            LoginFailureKind.None when result.Auth is not null => ApiResults.OkData(result.Auth),
            LoginFailureKind.AccountLocked => ApiResults.ProblemWithCode(
                ErrorCodes.AuthAccountLocked,
                "Account locked",
                StatusCodes.Status423Locked,
                detail: "Too many failed attempts. Try again later."),
            LoginFailureKind.EmailNotConfirmed => ApiResults.ProblemWithCode(
                ErrorCodes.AuthEmailNotConfirmed,
                "Email not confirmed",
                StatusCodes.Status403Forbidden,
                detail: "Confirm your email before signing in."),
            _ => ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Invalid credentials",
                StatusCodes.Status401Unauthorized,
                detail: "Invalid credentials."),
        };
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest dto,
        IValidator<ForgotPasswordRequest> validator,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResults.ValidationFailed(validation);
        }

        await auth.RequestPasswordResetAsync(dto.Email, cancellationToken);
        return ApiResults.OkData(
            new { message = "If an account exists for that email, password reset instructions were sent." });
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest dto,
        IValidator<ResetPasswordRequest> validator,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResults.ValidationFailed(validation);
        }

        var ok = await auth.ResetPasswordAsync(dto, cancellationToken);
        if (!ok)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidOrExpiredToken,
                "Invalid or expired token",
                StatusCodes.Status400BadRequest);
        }

        return ApiResults.OkData(new { message = "Password updated. You can sign in." });
    }

    private static async Task<IResult> ResendVerificationAsync(
        ResendVerificationRequest dto,
        IValidator<ResendVerificationRequest> validator,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResults.ValidationFailed(validation);
        }

        await auth.RequestResendVerificationAsync(dto.Email, cancellationToken);
        return ApiResults.OkData(new { message = "If the account exists and is unverified, a new confirmation link was sent." });
    }

    private static async Task<IResult> VerifyEmailAsync(
        string? token,
        IAuthService auth,
        IOptions<AuthPublicUrlsOptions> urls,
        CancellationToken cancellationToken)
    {
        var ok = await auth.VerifyEmailAsync(token ?? string.Empty, cancellationToken);
        var web = urls.Value.WebBaseUrl.TrimEnd('/');
        var suffix = ok ? "emailVerified=1" : "emailVerified=0";
        return Results.Redirect($"{web}/login?{suffix}");
    }

    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal user,
        IAuthService auth,
        CancellationToken cancellationToken)
    {
        var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.AuthInvalidCredentials,
                "Invalid credentials",
                StatusCodes.Status401Unauthorized,
                detail: "Invalid credentials.");
        }

        var me = await auth.GetCurrentUserAsync(userId, cancellationToken);
        if (me is null)
        {
            return ApiResults.ProblemWithCode(
                ErrorCodes.UserNotFound,
                "User not found",
                StatusCodes.Status404NotFound);
        }

        return ApiResults.OkData(me);
    }
}
