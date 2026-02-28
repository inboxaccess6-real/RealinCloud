using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Properties.Commands;

public record SubmitPropertyForReviewCommand(Guid PropertyId) : IRequest<bool>;

public class SubmitPropertyForReviewHandler : IRequestHandler<SubmitPropertyForReviewCommand, bool>
{
    private readonly IPropertyRepository _propertyRepository;

    public SubmitPropertyForReviewHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<bool> Handle(SubmitPropertyForReviewCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId, cancellationToken);
        if (property is null) throw new NotFoundException(nameof(Domain.Entities.Property), command.PropertyId);

        property.ApprovalStatus = "submitted";
        property.SubmittedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;

        await _propertyRepository.UpdateAsync(property, cancellationToken);

        return true;
    }
}
