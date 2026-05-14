using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MythicNexus.Application.Errors;
using MythicNexus.Application.Users.Contracts;
using MythicNexus.Application.Users.DTOs;
using MythicNexus.Domain.Entities;
using MythicNexus.Infrastructure.Persistence;
using Npgsql;

namespace MythicNexus.Application.Users.Services;

public sealed class AuthService : IAuthService
{
    /// <summary>Used only to keep password verification work roughly constant when the user row is missing.</summary>
    private static readonly string BcryptDummy = BCrypt.Net.BCrypt.HashPassword("Ln8WQjtZ!dummy-timing", workFactor: 8);

    private readonly MythicNexusDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailOutbox _emailOutbox;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AccountLockoutOptions _lockout;
    private readonly AuthPublicUrlsOptions _urls;

    public AuthService(
        MythicNexusDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailOutbox emailOutbox,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AccountLockoutOptions> lockout,
        IOptions<AuthPublicUrlsOptions> urls)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailOutbox = emailOutbox;
        _httpContextAccessor = httpContextAccessor;
        _lockout = lockout.Value;
        _urls = urls.Value;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var username = request.Username.Trim();

        if (await _db.Users.AsNoTracking().AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new DuplicateUserException(
                ErrorCodes.AuthEmailAlreadyExists,
                "This email is already registered.");
        }

        if (await _db.Users.AsNoTracking().AnyAsync(u => u.Username == username, cancellationToken))
        {
            throw new DuplicateUserException(
                ErrorCodes.AuthUsernameTaken,
                "This username is already taken.");
        }

        try
        {
            var tenant = await AllocateTenantAsync(username, cancellationToken);
            _db.Tenants.Add(tenant);

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = email,
                Username = username,
                PasswordHash = _passwordHasher.Hash(request.Password),
                CreatedAt = DateTimeOffset.UtcNow,
                EmailConfirmed = false,
                AccessFailedCount = 0,
            };

            _db.Users.Add(user);

            _db.TenantMemberships.Add(
                new TenantMembership
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    UserId = user.Id,
                    Role = TenantRole.Owner,
                    CreatedAt = DateTimeOffset.UtcNow,
                });

            var rawToken = CreateOpaqueToken();
            var tokenHash = Sha256Hex(rawToken);
            _db.EmailVerificationTokens.Add(
                new EmailVerificationToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash = tokenHash,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(48),
                });

            await _db.SaveChangesAsync(cancellationToken);

            var verifyUrl =
                $"{TrimSlash(_urls.ApiBaseUrl)}/api/users/verify-email?token={Uri.EscapeDataString(rawToken)}";
            var body =
                $"Welcome to MythicNexus, {username}.\r\n\r\nConfirm your email:\r\n{verifyUrl}\r\n\r\nThis link expires in 48 hours.";
            await _emailOutbox.WriteMessageAsync(
                "verify-email",
                "Confirm your MythicNexus email",
                body,
                email,
                cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new DuplicateUserException(
                ErrorCodes.AuthRegistrationConflict,
                "This email or username is already registered.");
        }

        return new RegisterResult
        {
            RequiresEmailVerification = true,
            AccessToken = null,
            Message = "Check your inbox to confirm your email before signing in.",
        };
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var ip = ClientIp();
        var userAgent = ClientUserAgent();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        var passwordOk = _passwordHasher.Verify(request.Password, user?.PasswordHash ?? BcryptDummy);

        if (user is null || !passwordOk)
        {
            await AppendLoginAuditAsync(
                email,
                success: false,
                failure: user is null ? "invalid_credentials" : "invalid_credentials",
                user?.Id,
                user?.TenantId,
                ip,
                userAgent,
                cancellationToken);

            if (user is not null)
            {
                await RegisterFailedAttemptAsync(user, cancellationToken);
            }

            return new LoginResult { Failure = LoginFailureKind.InvalidCredentials };
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            await AppendLoginAuditAsync(
                email,
                success: false,
                failure: "account_locked",
                user.Id,
                user.TenantId,
                ip,
                userAgent,
                cancellationToken);
            return new LoginResult { Failure = LoginFailureKind.AccountLocked };
        }

        if (!user.EmailConfirmed)
        {
            await AppendLoginAuditAsync(
                email,
                success: false,
                failure: "email_not_confirmed",
                user.Id,
                user.TenantId,
                ip,
                userAgent,
                cancellationToken);
            return new LoginResult { Failure = LoginFailureKind.EmailNotConfirmed };
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastSuccessfulLoginAt = DateTimeOffset.UtcNow;
        user.LastLoginIp = ip;

        await AppendLoginAuditAsync(
            email,
            success: true,
            failure: null,
            user.Id,
            user.TenantId,
            ip,
            userAgent,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return new LoginResult
        {
            Auth = new AuthResponse { AccessToken = _jwtTokenService.CreateAccessToken(user) },
            Failure = LoginFailureKind.None,
        };
    }

    public async Task<UserMeResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new UserMeResponse
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            Username = user.Username,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
        };
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        await Task.Delay(Random.Shared.Next(40, 120), cancellationToken);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
        if (user is null)
        {
            return;
        }

        var existing = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.ConsumedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
        foreach (var t in existing)
        {
            t.ConsumedAt = DateTimeOffset.UtcNow;
        }

        var rawToken = CreateOpaqueToken();
        var tokenHash = Sha256Hex(rawToken);
        _db.PasswordResetTokens.Add(
            new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            });

        await _db.SaveChangesAsync(cancellationToken);

        var resetUrl =
            $"{TrimSlash(_urls.WebBaseUrl)}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        var body =
            $"Password reset was requested for {user.Username}.\r\n\r\nOpen this link in the browser (expires in 2 hours):\r\n{resetUrl}\r\n\r\nIf you did not request this, ignore this message.";
        await _emailOutbox.WriteMessageAsync(
            "reset-password",
            "MythicNexus password reset",
            body,
            user.Email,
            cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var hash = Sha256Hex(request.Token.Trim());
        var row = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == hash && t.ConsumedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

        if (row is null)
        {
            return false;
        }

        row.User.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        row.ConsumedAt = DateTimeOffset.UtcNow;
        row.User.AccessFailedCount = 0;
        row.User.LockoutEnd = null;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var hash = Sha256Hex(token.Trim());
        var row = await _db.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == hash && t.ConsumedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

        if (row is null)
        {
            return false;
        }

        row.User.EmailConfirmed = true;
        row.User.EmailConfirmedAt = DateTimeOffset.UtcNow;
        row.ConsumedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RequestResendVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        await Task.Delay(Random.Shared.Next(40, 120), cancellationToken);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
        if (user is null || user.EmailConfirmed)
        {
            return;
        }

        var existing = await _db.EmailVerificationTokens
            .Where(t => t.UserId == user.Id && t.ConsumedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
        foreach (var t in existing)
        {
            t.ConsumedAt = DateTimeOffset.UtcNow;
        }

        var rawToken = CreateOpaqueToken();
        var tokenHash = Sha256Hex(rawToken);
        _db.EmailVerificationTokens.Add(
            new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(48),
            });

        await _db.SaveChangesAsync(cancellationToken);

        var verifyUrl =
            $"{TrimSlash(_urls.ApiBaseUrl)}/api/users/verify-email?token={Uri.EscapeDataString(rawToken)}";
        var body = $"Confirm your email:\r\n{verifyUrl}\r\n";
        await _emailOutbox.WriteMessageAsync(
            "verify-email-resend",
            "Confirm your MythicNexus email",
            body,
            user.Email,
            cancellationToken);
    }

    public async Task<TenantSummaryResponse?> GetTenantForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Join(_db.Tenants.AsNoTracking(), u => u.TenantId, t => t.Id, (u, t) => new { t.Id, t.Name, t.Slug })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new TenantSummaryResponse { Id = row.Id, Name = row.Name, Slug = row.Slug };
    }

    private async Task<Tenant> AllocateTenantAsync(string username, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var slugBase = Slugify(username);
            var slug = attempt == 0 ? slugBase : $"{slugBase}-{Random.Shared.Next(100000, 999999)}";
            if (await _db.Tenants.AsNoTracking().AnyAsync(t => t.Slug == slug, cancellationToken))
            {
                continue;
            }

            return new Tenant
            {
                Id = Guid.NewGuid(),
                Name = $"{username}'s workspace",
                Slug = slug,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }

        throw new InvalidOperationException("Could not allocate a unique tenant slug.");
    }

    private async Task RegisterFailedAttemptAsync(User user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;
        if (user.AccessFailedCount >= _lockout.MaxFailedAccessAttempts)
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(_lockout.LockoutMinutes);
            user.AccessFailedCount = 0;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task AppendLoginAuditAsync(
        string emailNormalized,
        bool success,
        string? failure,
        Guid? userId,
        Guid? tenantId,
        string? ip,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        _db.LoginAuditEvents.Add(
            new LoginAuditEvent
            {
                Id = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow,
                EmailNormalized = emailNormalized,
                Success = success,
                FailureReason = failure,
                IpAddress = ip,
                UserAgent = Truncate(userAgent, 512),
                UserId = userId,
                TenantId = tenantId,
            });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private string? ClientIp() =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    private string? ClientUserAgent()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx?.Request.Headers.TryGetValue("User-Agent", out var ua) != true)
        {
            return null;
        }

        return ua.ToString();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private static string CreateOpaqueToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Sha256Hex(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string TrimSlash(string url) => url.TrimEnd('/');

    private static string Slugify(string username)
    {
        var lower = username.Trim().ToLowerInvariant();
        var sb = new StringBuilder(lower.Length);
        foreach (var c in lower)
        {
            if (char.IsAsciiLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (c is '_' or '-' or '.')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('-');
            }
        }

        var s = sb.ToString().Trim('-');
        while (s.Contains("--", StringComparison.Ordinal))
        {
            s = s.Replace("--", "-", StringComparison.Ordinal);
        }

        if (s.Length < 2)
        {
            s = "user-" + Random.Shared.Next(10000, 99999);
        }

        return s.Length > 72 ? s[..72].TrimEnd('-') : s;
    }

    private static string? Truncate(string? s, int max) =>
        s is null ? null : s.Length <= max ? s : s[..max];
}
