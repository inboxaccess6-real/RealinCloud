using MediatR;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtService jwtService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

        if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new BusinessRuleException("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsBlocked)
            throw new BusinessRuleException("Account is inactive or blocked.");

        // Revoke old refresh token
        existingToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        // Generate new tokens
        var (accessToken, accessExpiresAt) = _jwtService.GenerateAccessToken(user);
        var (newRefreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        // Persist new refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            AccessTokenExpiresAt: accessExpiresAt,
            RefreshToken: newRefreshToken,
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
