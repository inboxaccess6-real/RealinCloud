using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Properties.Queries;

public record GetPropertiesQuery(
    int Page = 1,
    int PageSize = 50,
    string? City = null,
    bool? IsPublished = null,
    string? ApprovalStatus = null,
    Guid? AgentId = null
) : IRequest<PagedResult<PropertyResponse>>;

public class GetPropertiesHandler : IRequestHandler<GetPropertiesQuery, PagedResult<PropertyResponse>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertiesHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PagedResult<PropertyResponse>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _propertyRepository.GetPagedAsync(
            request.Page, request.PageSize, request.City, request.IsPublished, request.ApprovalStatus, request.AgentId, cancellationToken);

        var responses = items.Select(p => new PropertyResponse(
            p.Id, p.Title, p.ListingType, p.ListingCategory,
            p.PropertyType, p.ConstructionStatus, p.Facing,
            p.Price, p.Status, p.AgentId, p.ProjectId,
            p.ReraId, p.AvailableFrom,
            p.Address, p.Locality, p.City, p.PinCode, p.Landmark,
            p.Latitude, p.Longitude,
            p.FloorNumber, p.TotalFloors,
            p.Currency, p.MonthlyRent, p.SecurityDeposit, p.MaintenanceCharges,
            p.PriceNegotiable,
            p.CarpetArea, p.BuiltupArea, p.SuperBuiltupArea,
            p.Bedrooms, p.Bathrooms, p.Balconies,
            p.FurnishingStatus, p.PropertyAge,
            p.OwnershipType, p.LoanAvailable,
            p.VideoUrl, p.ImageUrl,
            p.Amenities, p.InteriorFeatures, p.Utilities,
            p.IsPublished, p.IsFeatured,
            p.ApprovalStatus, p.RejectionReason,
            p.IsFlagged, p.FlagReason,
            p.IsDeleted,
            p.CreatedAt, p.UpdatedAt,
            Agent: p.Agent is not null
                ? new AgentSummary(p.Agent.Id, p.Agent.AgencyName, p.Agent.Rating)
                : null,
            Project: p.Project is not null
                ? new ProjectSummary(p.Project.Id, p.Project.Name, p.Project.City, p.Project.BuilderId)
                : null
        )).ToList();

        return new PagedResult<PropertyResponse>(responses, totalCount, request.Page, request.PageSize);
    }
}
