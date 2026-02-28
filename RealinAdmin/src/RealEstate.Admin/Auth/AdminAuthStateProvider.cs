using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RealEstate.Admin.Services;
using RealEstate.Application.DTOs.Auth;

namespace RealEstate.Admin.Auth;

public class AdminAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _tokenService;
    private Func<Task<bool>>? _refreshTokenFunc;
    private UserInfo? _cachedUser;

    public AdminAuthStateProvider(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public UserInfo? CurrentUser => _cachedUser;

    /// <summary>
    /// Called once at startup to wire up the refresh callback (avoids circular DI).
    /// </summary>
    public void SetRefreshTokenFunc(Func<Task<bool>> refreshFunc)
    {
        _refreshTokenFunc = refreshFunc;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenService.GetAccessTokenAsync();
            var userJson = await _tokenService.GetUserInfoJsonAsync();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userJson))
            {
                _cachedUser = null;
                return Anonymous();
            }

            // Check if token is expired — try refresh if so
            var expiry = await _tokenService.GetAccessExpiryAsync();
            if (expiry.HasValue && DateTimeOffset.UtcNow >= expiry.Value)
            {
                if (_refreshTokenFunc != null && await _refreshTokenFunc())
                {
                    // Refresh succeeded — re-read the updated token/user from storage
                    token = await _tokenService.GetAccessTokenAsync();
                    userJson = await _tokenService.GetUserInfoJsonAsync();

                    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userJson))
                    {
                        _cachedUser = null;
                        return Anonymous();
                    }
                }
                else
                {
                    // Refresh failed or unavailable — clear stale tokens
                    await _tokenService.ClearTokensAsync();
                    _cachedUser = null;
                    return Anonymous();
                }
            }

            var user = JsonSerializer.Deserialize<UserInfo>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (user == null)
            {
                _cachedUser = null;
                return Anonymous();
            }

            _cachedUser = user;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role),
                new("role_level", user.RoleLevel.ToString()),
            };

            if (!string.IsNullOrEmpty(user.Name))
                claims.Add(new Claim(ClaimTypes.Name, user.Name));
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim("phone_number", user.PhoneNumber));

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            _cachedUser = null;
            return Anonymous();
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static AuthenticationState Anonymous()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));
}
