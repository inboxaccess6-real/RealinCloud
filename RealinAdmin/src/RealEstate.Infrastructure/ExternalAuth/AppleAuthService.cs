using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.ExternalAuth;

public class AppleAuthService : IAppleAuthService
{
    private const string AppleKeysUrl = "https://appleid.apple.com/auth/keys";
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string CacheKey = "ApplePublicKeys";

    private readonly string? _clientId;
    private readonly ILogger<AppleAuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    public AppleAuthService(
        IConfiguration configuration,
        ILogger<AppleAuthService> logger,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache)
    {
        _clientId = configuration["OAuth:Apple:ClientId"];
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public async Task<AppleUserPayload?> ValidateAppleTokenAsync(string idToken)
    {
        try
        {
            var keys = await GetApplePublicKeysAsync();
            if (keys is null || keys.Keys.Count == 0)
            {
                _logger.LogError("Failed to retrieve Apple public keys.");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(idToken);
            var kid = jwtToken.Header.Kid;

            var matchingKey = keys.Keys.FirstOrDefault(k => k.Kid == kid);
            if (matchingKey is null)
            {
                _logger.LogWarning("No matching Apple public key found for kid: {Kid}", kid);
                return null;
            }

            var rsaParameters = new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(matchingKey.N),
                Exponent = Base64UrlEncoder.DecodeBytes(matchingKey.E)
            };

            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParameters);
            var securityKey = new RsaSecurityKey(rsa) { KeyId = matchingKey.Kid };

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AppleIssuer,
                ValidateAudience = true,
                ValidAudience = _clientId,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out _);

            var sub = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var name = principal.FindFirst("name")?.Value;

            if (sub is null)
            {
                _logger.LogWarning("Apple token validation succeeded but 'sub' claim is missing.");
                return null;
            }

            return new AppleUserPayload(sub, email, name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Apple ID token.");
            return null;
        }
    }

    private async Task<ApplePublicKeys?> GetApplePublicKeysAsync()
    {
        if (_cache.TryGetValue(CacheKey, out ApplePublicKeys? cachedKeys))
        {
            return cachedKeys;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync(AppleKeysUrl);
            var keys = JsonSerializer.Deserialize<ApplePublicKeys>(response);

            if (keys is not null)
            {
                _cache.Set(CacheKey, keys, TimeSpan.FromHours(24));
            }

            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Apple public keys.");
            return null;
        }
    }
}

public class ApplePublicKeys
{
    [JsonPropertyName("keys")]
    public List<ApplePublicKey> Keys { get; set; } = new();
}

public class ApplePublicKey
{
    [JsonPropertyName("kty")]
    public string Kty { get; set; } = string.Empty;

    [JsonPropertyName("kid")]
    public string Kid { get; set; } = string.Empty;

    [JsonPropertyName("use")]
    public string Use { get; set; } = string.Empty;

    [JsonPropertyName("alg")]
    public string Alg { get; set; } = string.Empty;

    [JsonPropertyName("n")]
    public string N { get; set; } = string.Empty;

    [JsonPropertyName("e")]
    public string E { get; set; } = string.Empty;
}
