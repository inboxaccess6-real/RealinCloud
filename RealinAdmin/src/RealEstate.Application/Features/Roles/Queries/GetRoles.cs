using MediatR;
using RealEstate.Application.DTOs.Roles;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Roles.Queries;

public record GetRolesQuery() : IRequest<List<RoleResponse>>;

public class GetRolesHandler : IRequestHandler<GetRolesQuery, List<RoleResponse>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllWithUserCountAsync(cancellationToken);

        return roles.Select(r => new RoleResponse(
            r.Role.Id,
            r.Role.Name,
            r.Role.Description,
            r.Role.IsActive,
            r.Role.CreatedAt,
            r.Role.UpdatedAt,
            UserCount: r.UserCount
        )).ToList();
    }
}
