using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Roles.Commands;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;

public class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IRoleRepository _roleRepository;

    public DeleteRoleHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.Id, cancellationToken);

        if (role is null)
            throw new NotFoundException(nameof(Domain.Entities.Role), command.Id);

        // Prevent deleting roles that have assigned users
        if (role.Users.Any())
            throw new BusinessRuleException($"Cannot delete role '{role.Name}' because it has {role.Users.Count} assigned user(s).");

        await _roleRepository.DeleteAsync(role, cancellationToken);

        return true;
    }
}
