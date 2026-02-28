namespace RealinApi.Infrastructure.Authentication;

public record TokenResult(
    string Token,
    DateTimeOffset ExpiresAt
);