namespace MythicNexus.Application.Users;

public sealed class EmailOutboxOptions
{
    public const string SectionName = "Email:LocalOutbox";

    /// <summary>Directory name under the API content root where .eml-style drops are written until SMTP is wired.</summary>
    public string RelativeDirectory { get; set; } = "email-outbox";
}
