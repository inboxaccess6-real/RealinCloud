namespace RealinApi.Features.Auth.Models;

public record GoogleLoginRequest(string IdToken, string? DeviceInfo);

public record AppleLoginRequest(string IdToken, string? Name, string? DeviceInfo);

public record OtpRequestRequest(string? Email, string? PhoneNumber, string Method);

public record OtpVerifyRequest(string? Email, string? PhoneNumber, string Code, string? DeviceInfo);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Email,
    string? PhoneNumber,
    string? Name,
    string Provider,
    string Role,
    int RoleLevel
);
