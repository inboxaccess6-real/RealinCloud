using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace RealinApi.Infrastructure.ExternalServices;

public interface IAppleAuthService
{
    Task<AppleIdTokenPayload?> ValidateAppleTokenAsync(string idToken);
}

public record AppleIdTokenPayload(
    string Sub,
    string Email,
    bool EmailVerified,
    string? Name
);

public class AppleAuthService : IAppleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppleAuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    
    private const string ApplePublicKeysUrl = "https://appleid.apple.com/auth/keys";
    private const string AppleKeysCacheKey = "ApplePublicKeys";
    private static readonly TimeSpan KeysCacheDuration = TimeSpan.FromHours(24);

    public AppleAuthService(
        IConfiguration configuration, 
        ILogger<AppleAuthService> logger,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public async Task<AppleIdTokenPayload?> ValidateAppleTokenAsync(string idToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            
            // Read token without validation first to get the kid (key ID)
            var jwtToken = handler.ReadJwtToken(idToken);
            var kid = jwtToken.Header.Kid;

            if (string.IsNullOrEmpty(kid))
            {
                _logger.LogWarning("Apple ID token missing 'kid' in header");
                return null;
            }

            // Get Apple's public keys
            var publicKeys = await GetApplePublicKeysAsync();
            if (publicKeys == null)
            {
                _logger.LogError("Failed to fetch Apple public keys");
                return null;
            }

            // Find the matching key
            var key = publicKeys.Keys?.FirstOrDefault(k => k.Kid == kid);
            if (key == null)
            {
                _logger.LogWarning("No matching public key found for kid: {Kid}", kid);
                return null;
            }

            // Create security key from the JWK
            var rsa = new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                Exponent = Base64UrlEncoder.DecodeBytes(key.E)
            };

            var securityKey = new RsaSecurityKey(rsa) { KeyId = key.Kid };

            // Validate token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = _configuration["Apple:ClientId"], // Your app's bundle ID
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = handler.ValidateToken(idToken, validationParameters, out var validatedToken);

            // Extract claims
            var sub = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var emailVerified = principal.FindFirst("email_verified")?.Value;
            var name = principal.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Apple token missing required claims (sub or email)");
                return null;
            }

            return new AppleIdTokenPayload(
                sub,
                email,
                bool.Parse(emailVerified ?? "true"), // Apple emails are verified by default
                name
            );
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Apple ID token has expired");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning(ex, "Apple ID token has invalid signature");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Apple ID token");
            return null;
        }
    }

    private async Task<ApplePublicKeys?> GetApplePublicKeysAsync()
    {
        // Try to get from cache first
        if (_cache.TryGetValue(AppleKeysCacheKey, out ApplePublicKeys? cachedKeys))
        {
            return cachedKeys;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(ApplePublicKeysUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch Apple public keys. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var keys = JsonSerializer.Deserialize<ApplePublicKeys>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (keys != null)
            {
                // Cache the keys for 24 hours
                _cache.Set(AppleKeysCacheKey, keys, KeysCacheDuration);
            }

            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching Apple public keys");
            return null;
        }
    }
}

// Models for Apple's JWKS response
public class ApplePublicKeys
{
    public List<ApplePublicKey>? Keys { get; set; }
}

public class ApplePublicKey
{
    public string? Kty { get; set; }  // Key type (RSA)
    public string? Kid { get; set; }  // Key ID
    public string? Use { get; set; }  // Public key use (sig)
    public string? Alg { get; set; }  // Algorithm (RS256)
    public string? N { get; set; }    // Modulus
    public string? E { get; set; }    // Exponent
}
