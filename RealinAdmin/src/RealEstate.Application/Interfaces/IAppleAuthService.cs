namespace RealEstate.Application.Interfaces;

public interface IAppleAuthService
{
    Task<AppleUserPayload?> ValidateAppleTokenAsync(string idToken);
}

public record AppleUserPayload(string Sub, string? Email, string? Name);
