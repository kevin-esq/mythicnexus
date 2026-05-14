namespace MythicNexus.Application.Users.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
