using MediatR;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Users.Commands;

public record UpdateUserCommand(Guid Id, UpdateUserRequest Request) : IRequest<UserResponse?>;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(Domain.Entities.User), command.Id);

        if (command.Request.Name is not null)
            user.Name = command.Request.Name;

        if (command.Request.Email is not null)
            user.Email = command.Request.Email;

        if (command.Request.PhoneNumber is not null)
            user.PhoneNumber = command.Request.PhoneNumber;

        if (command.Request.RoleId.HasValue)
            user.RoleId = command.Request.RoleId.Value;

        if (command.Request.IsActive.HasValue)
            user.IsActive = command.Request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.Name,
            user.Provider,
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
