using MediatR;
using RealEstate.Application.DTOs.Roles;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Roles.Commands;

public record UpdateRoleCommand(Guid Id, UpdateRoleRequest Request) : IRequest<RoleResponse?>;

public class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, RoleResponse?>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse?> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.Id, cancellationToken);

        if (role is null)
            throw new NotFoundException(nameof(Domain.Entities.Role), command.Id);

        if (command.Request.Name is not null)
        {
            // Check for duplicate name
            var existing = await _roleRepository.GetByNameAsync(command.Request.Name, cancellationToken);
            if (existing is not null && existing.Id != command.Id)
                throw new BusinessRuleException($"A role with name '{command.Request.Name}' already exists.");

            role.Name = command.Request.Name;
        }

        if (command.Request.Description is not null)
            role.Description = command.Request.Description;

        if (command.Request.IsActive.HasValue)
            role.IsActive = command.Request.IsActive.Value;

        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role, cancellationToken);

        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            UserCount: 0 // TODO: Actual user count requires a dedicated repo method
        );
    }
}
