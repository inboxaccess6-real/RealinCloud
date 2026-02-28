using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Users.Commands;

public record UnblockUserCommand(Guid UserId) : IRequest<bool>;

public class UnblockUserHandler : IRequestHandler<UnblockUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public UnblockUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(UnblockUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(Domain.Entities.User), command.UserId);

        user.IsBlocked = false;
        user.BlockedBy = null;
        user.BlockedAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }
}
