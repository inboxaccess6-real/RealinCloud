namespace RealinApi.Features.Auth.Models;

using System.Text.Json.Serialization;

public record GoogleLoginRequest(string IdToken, string? DeviceInfo);

public record AppleLoginRequest(string IdToken, string? Name, string? DeviceInfo);

public record OtpRequestRequest(LoginMethod Method, string Recipient);

public record OtpVerifyRequest(LoginMethod Method, string Recipient, string Code, string? DeviceInfo);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string? Email,
    string? PhoneNumber,
    string? Name,
    string Provider,
    string Role,
    int RoleLevel
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoginMethod
{
    Email,
    Mobile
}