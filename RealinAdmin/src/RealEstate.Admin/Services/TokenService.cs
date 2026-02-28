using Microsoft.JSInterop;

namespace RealEstate.Admin.Services;

public class TokenService
{
    private readonly IJSRuntime _js;

    private const string AccessTokenKey = "realin_access_token";
    private const string RefreshTokenKey = "realin_refresh_token";
    private const string AccessExpiryKey = "realin_access_expiry";
    private const string RefreshExpiryKey = "realin_refresh_expiry";
    private const string UserInfoKey = "realin_user_info";

    public TokenService(IJSRuntime js) => _js = js;

    public async Task SaveTokensAsync(string accessToken, DateTimeOffset accessExpiry, string refreshToken, DateTimeOffset refreshExpiry)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, accessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
        await _js.InvokeVoidAsync("localStorage.setItem", AccessExpiryKey, accessExpiry.ToString("o"));
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshExpiryKey, refreshExpiry.ToString("o"));
    }

    public async Task SaveUserInfoAsync(string userInfoJson)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", UserInfoKey, userInfoJson);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
    }

    public async Task<DateTimeOffset?> GetAccessExpiryAsync()
    {
        var value = await _js.InvokeAsync<string?>("localStorage.getItem", AccessExpiryKey);
        return value != null && DateTimeOffset.TryParse(value, out var expiry) ? expiry : null;
    }

    public async Task<string?> GetUserInfoJsonAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", UserInfoKey);
    }

    public async Task ClearTokensAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", AccessExpiryKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshExpiryKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", UserInfoKey);
    }

    public async Task<bool> IsAccessTokenExpiringSoonAsync(int bufferMinutes = 2)
    {
        var expiry = await GetAccessExpiryAsync();
        if (expiry == null) return true;
        return DateTimeOffset.UtcNow.AddMinutes(bufferMinutes) >= expiry.Value;
    }
}
