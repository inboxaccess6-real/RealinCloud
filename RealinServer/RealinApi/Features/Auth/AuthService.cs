using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;
using RealinApi.Features.Auth.Models;
using RealinApi.Infrastructure.Authentication;
using RealinApi.Infrastructure.ExternalServices;
using RealinApi.Infrastructure.Messaging;

namespace RealinApi.Features.Auth;

public interface IAuthService
{
    Task<(bool Success, AuthResponse? Response, string? Error)> GoogleLoginAsync(GoogleLoginRequest request);
    Task<(bool Success, AuthResponse? Response, string? Error)> AppleLoginAsync(AppleLoginRequest request);
    Task<(bool Success, string? Message, string? Error)> RequestOtpAsync(OtpRequestRequest request);
    Task<(bool Success, AuthResponse? Response, string? Error)> VerifyOtpAsync(OtpVerifyRequest request);
    Task<(bool Success, AuthResponse? Response, string? Error)> RefreshTokenAsync(RefreshTokenRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAppleAuthService _appleAuthService;
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IJwtService jwtService,
        IGoogleAuthService googleAuthService,
        IAppleAuthService appleAuthService,
        IOtpService otpService,
        ISmsService smsService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _googleAuthService = googleAuthService;
        _appleAuthService = appleAuthService;
        _otpService = otpService;
        _smsService = smsService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> GoogleLoginAsync(GoogleLoginRequest request)
    {
        // Validate Google token
        var payload = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken);
        if (payload == null)
        {
            return (false, null, "Invalid Google token");
        }

        // Check if user exists
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Provider == AuthProvider.Google && u.OAuthProviderId == payload.Subject);

        if (user == null)
        {
            // Get default role (User role)
            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleType == RoleType.User);
            
            if (defaultRole == null)
            {
                _logger.LogError("Default User role not found in database");
                return (false, null, "System configuration error");
            }

            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                Name = payload.Name,
                Provider = AuthProvider.Google,
                OAuthProviderId = payload.Subject,
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New Google user created: {UserId} with role: {RoleType}", user.Id, defaultRole.RoleType);
        }
        else
        {
            // Update existing user
            user.Name = payload.Name;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Generate tokens
        return await GenerateAuthResponseAsync(user, request.DeviceInfo);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> AppleLoginAsync(AppleLoginRequest request)
    {
        // Validate Apple token
        var payload = await _appleAuthService.ValidateAppleTokenAsync(request.IdToken);
        if (payload == null)
        {
            return (false, null, "Invalid Apple token");
        }

        // Check if user exists
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Provider == AuthProvider.Apple && u.OAuthProviderId == payload.Sub);

        if (user == null)
        {
            // Get default role (User role)
            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleType == RoleType.User);
            
            if (defaultRole == null)
            {
                _logger.LogError("Default User role not found in database");
                return (false, null, "System configuration error");
            }

            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                Name = request.Name ?? payload.Name,
                Provider = AuthProvider.Apple,
                OAuthProviderId = payload.Sub,
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New Apple user created: {UserId} with role: {RoleType}", user.Id, defaultRole.RoleType);
        }
        else
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Generate tokens
        return await GenerateAuthResponseAsync(user, request.DeviceInfo);
    }

    public async Task<(bool Success, string? Message, string? Error)> RequestOtpAsync(OtpRequestRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
        {
            return (false, null, "Email or phone number is required");
        }

        var method = request.Method.ToLower() switch
        {
            "sms" => OtpDeliveryMethod.Sms,
            "email" => OtpDeliveryMethod.Email,
            _ => (OtpDeliveryMethod?)null
        };

        if (method == null)
        {
            return (false, null, "Invalid delivery method. Use 'sms' or 'email'");
        }

        if (method == OtpDeliveryMethod.Sms && string.IsNullOrEmpty(request.PhoneNumber))
        {
            return (false, null, "Phone number is required for SMS delivery");
        }

        if (method == OtpDeliveryMethod.Email && string.IsNullOrEmpty(request.Email))
        {
            return (false, null, "Email is required for email delivery");
        }

        // Generate OTP
        var (success, code) = await _otpService.GenerateOtpAsync(request.Email, request.PhoneNumber, method.Value);
        if (!success || code == null)
        {
            return (false, null, "Rate limit exceeded. Please try again later");
        }

        // Send OTP
        if (method == OtpDeliveryMethod.Sms)
        {
            await _smsService.SendOtpAsync(request.PhoneNumber!, code);
        }
        else
        {
            await _emailService.SendOtpAsync(request.Email!, code);
        }

        return (true, "OTP sent successfully", null);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> VerifyOtpAsync(OtpVerifyRequest request)
    {
        // Validate OTP
        var (success, error, existingUserId) = await _otpService.ValidateOtpAsync(request.Email, request.PhoneNumber, request.Code);
        if (!success)
        {
            return (false, null, error);
        }

        // Find or create user
        User? user;
        
        if (existingUserId.HasValue)
        {
            user = await _context.Users.FindAsync(existingUserId.Value);
            if (user == null)
            {
                return (false, null, "User not found");
            }
        }
        else
        {
            // Check if user exists by email or phone
            user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Provider == AuthProvider.Otp &&
                                         ((!string.IsNullOrEmpty(request.Email) && u.Email == request.Email) ||
                                          (!string.IsNullOrEmpty(request.PhoneNumber) && u.PhoneNumber == request.PhoneNumber)));

            if (user == null)
            {
                // Get default role (User role)
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleType == RoleType.User);
                
                if (defaultRole == null)
                {
                    _logger.LogError("Default User role not found in database");
                    return (false, null, "System configuration error");
                }

                // Create new OTP user
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email ?? $"{request.PhoneNumber}@otp.local",
                    PhoneNumber = request.PhoneNumber,
                    Provider = AuthProvider.Otp,
                    RoleId = defaultRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New OTP user created: {UserId} with role: {RoleType}", user.Id, defaultRole.RoleType);
            }
            else
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Generate tokens
        return await GenerateAuthResponseAsync(user, request.DeviceInfo);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

        if (refreshToken == null)
        {
            return (false, null, "Invalid refresh token");
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return (false, null, "Refresh token has expired");
        }

        // Revoke old refresh token
        refreshToken.IsRevoked = true;

        // Generate new tokens
        var response = await GenerateAuthResponseAsync(refreshToken.User, refreshToken.DeviceInfo);
        await _context.SaveChangesAsync();

        return response;
    }

    private async Task<(bool Success, AuthResponse? Response, string? Error)> GenerateAuthResponseAsync(User user, string? deviceInfo)
    {
        // Ensure Role is loaded
        if (user.Role == null)
        {
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            
            // If still null, something is wrong
            if (user.Role == null)
            {
                _logger.LogError("User {UserId} has no associated role", user.Id);
                return (false, null, "User role configuration error");
            }
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: refreshTokenEntity.ExpiresAt,
            User: new UserInfo(
                Id: user.Id,
                Email: user.Email,
                PhoneNumber: user.PhoneNumber,
                Name: user.Name,
                Provider: user.Provider.ToString(),
                Role: user.Role.Name,
                RoleLevel: (int)user.Role.RoleType
            )
        );

        return (true, response, null);
    }
}
