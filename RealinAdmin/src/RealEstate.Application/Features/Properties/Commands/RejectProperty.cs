using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Properties.Commands;

public record RejectPropertyCommand(Guid PropertyId, Guid ReviewedBy, string Reason) : IRequest<bool>;

public class RejectPropertyHandler : IRequestHandler<RejectPropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public RejectPropertyHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(RejectPropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId, cancellationToken);
        if (property is null) throw new NotFoundException(nameof(Domain.Entities.Property), command.PropertyId);

        property.ApprovalStatus = "rejected";
        property.RejectionReason = command.Reason;
        property.ReviewedBy = command.ReviewedBy;
        property.ReviewedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;

        await _propertyRepository.UpdateAsync(property, cancellationToken);

        return true;
    }
}
