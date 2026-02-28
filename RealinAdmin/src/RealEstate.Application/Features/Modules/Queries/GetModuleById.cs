using MediatR;
using RealEstate.Application.DTOs.Modules;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Modules.Queries;

public record GetModuleByIdQuery(Guid Id) : IRequest<ModuleResponse>;

public class GetModuleByIdHandler : IRequestHandler<GetModuleByIdQuery, ModuleResponse>
{
    private readonly IModuleRepository _moduleRepository;

    public GetModuleByIdHandler(IModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<ModuleResponse> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
    {
        var module = await _moduleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (module is null)
            throw new NotFoundException(nameof(Domain.Entities.Module), request.Id);

        return new ModuleResponse(
            module.Id,
            module.Code,
            module.Name,
            module.Description,
            module.IsActive,
            module.CreatedAt,
            module.UpdatedAt,
            PermissionCount: module.RolePermissions?.Count ?? 0
        );
    }
}
