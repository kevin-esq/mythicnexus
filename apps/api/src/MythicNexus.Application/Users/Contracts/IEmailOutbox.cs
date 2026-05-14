namespace MythicNexus.Application.Users.Contracts;

public interface IEmailOutbox
{
    Task WriteMessageAsync(string fileNamePrefix, string subject, string body, string recipientEmail, CancellationToken cancellationToken = default);
}
