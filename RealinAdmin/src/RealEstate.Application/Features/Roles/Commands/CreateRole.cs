using MediatR;
using RealEstate.Application.DTOs.Roles;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Roles.Commands;

public record CreateRoleCommand(CreateRoleRequest Request) : IRequest<RoleResponse>;

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, RoleResponse>
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        // Validate role type
        if (!command.Request.Name.TryParseRoleType(out var roleType))
            throw new BusinessRuleException($"Invalid role name '{command.Request.Name}'. Must be a valid RoleType.");

        // Check for duplicates
        var existing = await _roleRepository.GetByNameAsync(command.Request.Name, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleException($"A role with name '{command.Request.Name}' already exists.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            RoleType = roleType,
            Name = command.Request.Name,
            Description = command.Request.Description,
            IsActive = command.Request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _roleRepository.AddAsync(role, cancellationToken);

        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            UserCount: 0
        );
    }
}
