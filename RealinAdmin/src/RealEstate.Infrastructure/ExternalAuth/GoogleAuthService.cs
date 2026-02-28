using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.ExternalAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly string? _clientId;

    public GoogleAuthService(IConfiguration configuration)
    {
        _clientId = configuration["OAuth:Google:ClientId"];
    }

    public async Task<GoogleUserPayload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _clientId is not null ? new[] { _clientId } : null
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserPayload(payload.Subject, payload.Email, payload.Name);
        }
        catch
        {
            return null;
        }
    }
}
