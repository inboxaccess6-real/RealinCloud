using MediatR;
using RealEstate.Application.DTOs.Modules;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Modules.Commands;

public record CreateModuleCommand(CreateModuleRequest Request) : IRequest<ModuleResponse>;

public class CreateModuleHandler : IRequestHandler<CreateModuleCommand, ModuleResponse>
{
    private readonly IModuleRepository _moduleRepository;

    public CreateModuleHandler(IModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<ModuleResponse> Handle(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var existing = await _moduleRepository.GetByCodeAsync(command.Request.Code, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleException($"A module with code '{command.Request.Code}' already exists.");

        var module = new Module
        {
            Id = Guid.NewGuid(),
            Code = command.Request.Code,
            Name = command.Request.Name,
            Description = command.Request.Description,
            IsActive = command.Request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _moduleRepository.AddAsync(module, cancellationToken);

        return new ModuleResponse(
            module.Id,
            module.Code,
            module.Name,
            module.Description,
            module.IsActive,
            module.CreatedAt,
            module.UpdatedAt,
            PermissionCount: 0
        );
    }
}
