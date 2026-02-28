using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Properties.Commands;

public record SubmitPropertyCommand(Guid PropertyId) : IRequest<bool>;

public class SubmitPropertyHandler : IRequestHandler<SubmitPropertyCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public SubmitPropertyHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(SubmitPropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId, cancellationToken);
        if (property is null) throw new NotFoundException(nameof(Domain.Entities.Property), command.PropertyId);

        if (property.ApprovalStatus != "draft" && property.ApprovalStatus != "rejected")
            throw new InvalidOperationException($"Property cannot be submitted for approval from '{property.ApprovalStatus}' status.");

        property.ApprovalStatus = "submitted";
        property.SubmittedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;
        property.RejectionReason = null;

        await _propertyRepository.UpdateAsync(property, cancellationToken);

        return true;
    }
}
