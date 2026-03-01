using MediatR;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserResponse?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return null;

        return new UserResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.Name,
            user.Provider,
            user.RoleId,
            user.Role?.Name ?? "",
            user.IsActive,
            user.IsBlocked,
            user.IsBlacklisted,
            user.BlacklistReason,
            user.IsDeleted,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
