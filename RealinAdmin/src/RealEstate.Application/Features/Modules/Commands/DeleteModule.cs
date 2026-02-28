using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Modules.Commands;

public record DeleteModuleCommand(Guid Id) : IRequest<bool>;

public class DeleteModuleHandler : IRequestHandler<DeleteModuleCommand, bool>
{
    private readonly IModuleRepository _moduleRepository;

    public DeleteModuleHandler(IModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<bool> Handle(DeleteModuleCommand command, CancellationToken cancellationToken)
    {
        var module = await _moduleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (module is null)
            throw new NotFoundException(nameof(Domain.Entities.Module), command.Id);

        await _moduleRepository.DeleteAsync(module, cancellationToken);

        return true;
    }
}
