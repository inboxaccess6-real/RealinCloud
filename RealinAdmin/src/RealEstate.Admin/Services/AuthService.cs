using System.Net.Http.Json;
using System.Text.Json;
using RealEstate.Admin.Auth;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Auth;

namespace RealEstate.Admin.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly AdminAuthStateProvider _authStateProvider;

    public AuthService(IHttpClientFactory httpClientFactory, TokenService tokenService, AdminAuthStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApi");
        _tokenService = tokenService;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string? Error)> RequestOtpAsync(LoginMethod method, string recipient)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/otp/request", new OtpRequestRequest(method, recipient));
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            var error = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return (false, error?.Error ?? "Failed to send OTP");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error, UserInfo? User)> VerifyOtpAsync(LoginMethod method, string recipient, string code, bool notifyAuthStateChanged = true)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/otp/verify",
                new OtpVerifyRequest(method, recipient, code, null));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                if (result?.Success == true && result.Data != null)
                {
                    await _tokenService.SaveTokensAsync(
                        result.Data.AccessToken,
                        result.Data.AccessTokenExpiresAt,
                        result.Data.RefreshToken,
                        result.Data.RefreshTokenExpiresAt);

                    var userJson = JsonSerializer.Serialize(result.Data.User);
                    await _tokenService.SaveUserInfoAsync(userJson);

                    if (notifyAuthStateChanged)
                        _authStateProvider.NotifyAuthenticationStateChanged();

                    return (true, null, result.Data.User);
                }
                return (false, result?.Error ?? "Login failed", null);
            }
            var error = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            return (false, error?.Error ?? "Invalid OTP", null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<bool> TryRefreshTokenAsync(bool notifyAuthState = true)
    {
        try
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh",
                new RefreshTokenRequest(refreshToken));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                if (result?.Success == true && result.Data != null)
                {
                    await _tokenService.SaveTokensAsync(
                        result.Data.AccessToken,
                        result.Data.AccessTokenExpiresAt,
                        result.Data.RefreshToken,
                        result.Data.RefreshTokenExpiresAt);

                    var userJson = JsonSerializer.Serialize(result.Data.User);
                    await _tokenService.SaveUserInfoAsync(userJson);

                    if (notifyAuthState)
                        _authStateProvider.NotifyAuthenticationStateChanged();
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _tokenService.ClearTokensAsync();
        _authStateProvider.NotifyAuthenticationStateChanged();
    }
}
