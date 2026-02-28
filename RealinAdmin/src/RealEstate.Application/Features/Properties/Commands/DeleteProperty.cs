using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Properties.Commands;

public record DeletePropertyCommand(Guid Id, Guid? DeletedBy = null) : IRequest<bool>;

public class DeletePropertyHandler : IRequestHandler<DeletePropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public DeletePropertyHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(DeletePropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.Id, cancellationToken);
        if (property is null) throw new NotFoundException(nameof(Domain.Entities.Property), command.Id);

        property.IsDeleted = true;
        property.DeletedAt = DateTime.UtcNow;
        property.DeletedBy = command.DeletedBy;
        property.UpdatedAt = DateTime.UtcNow;

        await _propertyRepository.UpdateAsync(property, cancellationToken);

        return true;
    }
}
