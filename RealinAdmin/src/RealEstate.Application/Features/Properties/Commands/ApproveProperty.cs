using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Properties.Commands;

public record ApprovePropertyCommand(Guid PropertyId, Guid ReviewedBy) : IRequest<bool>;

public class ApprovePropertyHandler : IRequestHandler<ApprovePropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public ApprovePropertyHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(ApprovePropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId, cancellationToken);
        if (property is null) throw new NotFoundException(nameof(Domain.Entities.Property), command.PropertyId);

        property.ApprovalStatus = "approved";
        property.ReviewedBy = command.ReviewedBy;
        property.ReviewedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;

        await _propertyRepository.UpdateAsync(property, cancellationToken);

        return true;
    }
}
