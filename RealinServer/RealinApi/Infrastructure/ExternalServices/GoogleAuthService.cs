using Google.Apis.Auth;

namespace RealinApi.Infrastructure.ExternalServices;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken);
}

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string? _googleClientId;

    public GoogleAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        _googleClientId = configuration["OAuth:Google:ClientId"];
    }

    public async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _googleClientId != null ? new[] { _googleClientId } : null
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
