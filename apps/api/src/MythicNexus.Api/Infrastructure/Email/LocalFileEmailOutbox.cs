using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MythicNexus.Application.Users;
using MythicNexus.Application.Users.Contracts;

namespace MythicNexus.Api.Infrastructure.Email;

public sealed class LocalFileEmailOutbox : IEmailOutbox
{
    private readonly IWebHostEnvironment _env;
    private readonly EmailOutboxOptions _options;

    public LocalFileEmailOutbox(IWebHostEnvironment env, IOptions<EmailOutboxOptions> options)
    {
        _env = env;
        _options = options.Value;
    }

    public async Task WriteMessageAsync(
        string fileNamePrefix,
        string subject,
        string body,
        string recipientEmail,
        CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_env.ContentRootPath, _options.RelativeDirectory);
        Directory.CreateDirectory(dir);
        var safePrefix = string.Join("_", fileNamePrefix.Split(Path.GetInvalidFileNameChars()));
        var name = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{safePrefix}_{Guid.NewGuid():N}.txt";
        var path = Path.Combine(dir, name);
        var text =
            $"To: {recipientEmail}\r\nSubject: {subject}\r\nDate: {DateTimeOffset.UtcNow:R}\r\n\r\n{body}";
        await File.WriteAllTextAsync(path, text, cancellationToken);
    }
}
