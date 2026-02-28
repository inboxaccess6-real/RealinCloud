using MediatR;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Auth.Commands;

public record GoogleLoginCommand(string IdToken, string? DeviceInfo) : IRequest<AuthResponse>;

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtService _jwtService;

    public GoogleLoginHandler(
        IGoogleAuthService googleAuthService,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtService jwtService)
    {
        _googleAuthService = googleAuthService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implementation deferred to Infrastructure layer - requires direct DbContext access for refresh token management
        // Outline:
        // 1. Validate Google token via IGoogleAuthService.ValidateGoogleTokenAsync
        // 2. Find existing user by email or create new user with default "User" role
        // 3. Generate access token and refresh token via IJwtService
        // 4. Persist refresh token to database
        // 5. Return AuthResponse with tokens and user info
        throw new NotImplementedException(
            "Implementation deferred to Infrastructure layer - requires direct DbContext access for refresh token management");
    }
}
