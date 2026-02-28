using MediatR;
using RealEstate.Application.DTOs.Roles;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Roles.Commands;

public record AssignPermissionsCommand(Guid RoleId, AssignPermissionsRequest Request) : IRequest<RoleResponse>;

public class AssignPermissionsHandler : IRequestHandler<AssignPermissionsCommand, RoleResponse>
{
    private readonly IRoleRepository _roleRepository;

    public AssignPermissionsHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse> Handle(AssignPermissionsCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
            throw new NotFoundException(nameof(Role), command.RoleId);

        // Step 1: Delete all existing permissions (uses ExecuteDeleteAsync, bypasses tracker)
        await _roleRepository.ClearPermissionsAsync(command.RoleId, cancellationToken);

        // Step 2: Build new permission entities
        var now = DateTime.UtcNow;
        var newPermissions = command.Request.Permissions.Select(a => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = command.RoleId,
            ModuleId = a.ModuleId,
            CanRead = a.CanRead,
            CanCreate = a.CanCreate,
            CanUpdate = a.CanUpdate,
            CanDelete = a.CanDelete,
            CanManage = a.CanManage,
            CanExport = a.CanExport,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        // Step 3: Insert new permissions directly (clean Add, no Update on graph)
        if (newPermissions.Count > 0)
        {
            await _roleRepository.AddPermissionsAsync(newPermissions, cancellationToken);
        }

        // Step 4: Update role timestamp
        role = (await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken))!;
        role.UpdatedAt = now;
        await _roleRepository.UpdateAsync(role, cancellationToken);

        // Re-fetch to get full module info
        var updated = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken);

        return new RoleResponse(
            updated!.Id,
            updated.Name,
            updated.Description,
            updated.IsActive,
            updated.CreatedAt,
            updated.UpdatedAt,
            UserCount: 0,
            Permissions: updated.Permissions?.Select(p => new RolePermissionDto(
                p.ModuleId,
                p.Module?.Code ?? "",
                p.Module?.Name ?? "",
                p.CanRead,
                p.CanCreate,
                p.CanUpdate,
                p.CanDelete,
                p.CanManage,
                p.CanExport
            )).ToList()
        );
    }
}
