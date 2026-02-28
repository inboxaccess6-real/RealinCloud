using MediatR;
using RealEstate.Application.DTOs.Modules;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Modules.Commands;

public record UpdateModuleCommand(Guid Id, UpdateModuleRequest Request) : IRequest<ModuleResponse>;

public class UpdateModuleHandler : IRequestHandler<UpdateModuleCommand, ModuleResponse>
{
    private readonly IModuleRepository _moduleRepository;

    public UpdateModuleHandler(IModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<ModuleResponse> Handle(UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        var module = await _moduleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (module is null)
            throw new NotFoundException(nameof(Domain.Entities.Module), command.Id);

        if (command.Request.Code is not null) module.Code = command.Request.Code;
        if (command.Request.Name is not null) module.Name = command.Request.Name;
        if (command.Request.Description is not null) module.Description = command.Request.Description;
        if (command.Request.IsActive.HasValue) module.IsActive = command.Request.IsActive.Value;
        module.UpdatedAt = DateTime.UtcNow;

        await _moduleRepository.UpdateAsync(module, cancellationToken);

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
