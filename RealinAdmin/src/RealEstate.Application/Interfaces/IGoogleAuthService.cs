namespace RealEstate.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserPayload?> ValidateGoogleTokenAsync(string idToken);
}

public record GoogleUserPayload(string Subject, string? Email, string? Name);
