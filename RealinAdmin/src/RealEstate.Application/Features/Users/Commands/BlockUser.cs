using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Users.Commands;

public record BlockUserCommand(Guid UserId, Guid BlockedByUserId) : IRequest<bool>;

public class BlockUserHandler : IRequestHandler<BlockUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public BlockUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(Domain.Entities.User), command.UserId);

        user.IsBlocked = true;
        user.BlockedBy = command.BlockedByUserId;
        user.BlockedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }
}
