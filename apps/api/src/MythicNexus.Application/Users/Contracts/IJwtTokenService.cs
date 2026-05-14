using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Users.Contracts;

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
}
