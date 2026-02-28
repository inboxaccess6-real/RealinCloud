using MediatR;
using RealEstate.Application.DTOs.Roles;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Roles.Queries;

public record GetRoleByIdQuery(Guid Id) : IRequest<RoleResponse?>;

public class GetRoleByIdHandler : IRequestHandler<GetRoleByIdQuery, RoleResponse?>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
            return null;

        var userCount = await _roleRepository.GetUserCountAsync(request.Id, cancellationToken);

        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            UserCount: userCount,
            Permissions: role.Permissions?.Select(p => new RolePermissionDto(
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
