namespace MythicNexus.Application.Users.Services;

public sealed class DuplicateUserException : Exception
{
    public string Code { get; }
    public string PublicMessage { get; }

    public DuplicateUserException(string code, string publicMessage)
        : base(publicMessage)
    {
        Code = code;
        PublicMessage = publicMessage;
    }
}
