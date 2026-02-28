using MediatR;
using RealEstate.Application.DTOs.Modules;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Modules.Queries;

public record GetModulesQuery() : IRequest<List<ModuleResponse>>;

public class GetModulesHandler : IRequestHandler<GetModulesQuery, List<ModuleResponse>>
{
    private readonly IModuleRepository _moduleRepository;

    public GetModulesHandler(IModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<List<ModuleResponse>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
    {
        var modules = await _moduleRepository.GetAllAsync(cancellationToken);

        return modules.Select(m => new ModuleResponse(
            m.Id,
            m.Code,
            m.Name,
            m.Description,
            m.IsActive,
            m.CreatedAt,
            m.UpdatedAt,
            PermissionCount: m.RolePermissions?.Count ?? 0
        )).ToList();
    }
}
