namespace RealEstate.Infrastructure.Identity;

public record TokenResult(string Token, DateTimeOffset ExpiresAt);
