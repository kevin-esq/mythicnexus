namespace MythicNexus.Domain.Entities;

/// <summary>
/// Append-only security log for authentication attempts (success and failure).
/// </summary>
public class LoginAuditEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string EmailNormalized { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
}
