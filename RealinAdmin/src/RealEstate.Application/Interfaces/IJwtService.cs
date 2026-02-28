using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    (string Token, DateTimeOffset ExpiresAt) GenerateRefreshToken();
}
