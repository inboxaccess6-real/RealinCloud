using MediatR;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Auth.Commands;

public record VerifyOtpCommand(LoginMethod Method, string Recipient, string Code, string? DeviceInfo) : IRequest<AuthResponse>;

public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, AuthResponse>
{
    private readonly IOtpService _otpService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public VerifyOtpHandler(
        IOtpService otpService,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _otpService = otpService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthResponse> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var deliveryMethod = command.Method == LoginMethod.Email
            ? OtpDeliveryMethod.Email
            : OtpDeliveryMethod.Sms;

        var (success, error, _) = await _otpService.ValidateOtpAsync(command.Recipient, deliveryMethod, command.Code);

        if (!success)
            throw new BusinessRuleException(error ?? "Invalid OTP.");

        // Find existing user by email or phone
        User? user = command.Method == LoginMethod.Email
            ? await _userRepository.GetByEmailAsync(command.Recipient, cancellationToken)
            : await _userRepository.GetByPhoneAsync(command.Recipient, cancellationToken);

        if (user is null)
        {
            // Auto-create user with default "User" role
            var defaultRole = await _roleRepository.GetByRoleTypeAsync(RoleType.User, cancellationToken);
            if (defaultRole is null)
                throw new BusinessRuleException("Default user role not found. Contact administrator.");

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Method == LoginMethod.Email ? command.Recipient : null,
                PhoneNumber = command.Method == LoginMethod.Mobile ? command.Recipient : null,
                Provider = AuthProvider.Otp,
                RoleId = defaultRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            user.Role = defaultRole;

            await _userRepository.AddAsync(user, cancellationToken);
        }

        if (!user.IsActive)
            throw new BusinessRuleException("Your account has been deactivated. Contact administrator.");

        if (user.IsBlocked)
            throw new BusinessRuleException("Your account has been blocked. Contact administrator.");

        // Generate tokens
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user);
        var (refreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        // Persist refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            DeviceInfo = command.DeviceInfo
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            AccessTokenExpiresAt: accessExpiresAt,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: refreshExpiresAt,
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
    }
}
