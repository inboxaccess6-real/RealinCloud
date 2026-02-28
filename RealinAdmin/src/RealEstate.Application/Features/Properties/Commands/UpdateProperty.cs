using MediatR;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Properties.Commands;

public record UpdatePropertyCommand(Guid Id, UpdatePropertyRequest Request) : IRequest<PropertyResponse?>;

public class UpdatePropertyHandler : IRequestHandler<UpdatePropertyCommand, PropertyResponse?>
{
    private readonly IPropertyRepository _propertyRepository;

    public UpdatePropertyHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyResponse?> Handle(UpdatePropertyCommand command, CancellationToken cancellationToken)
    {
        var p = await _propertyRepository.GetByIdAsync(command.Id, cancellationToken);
        if (p is null) throw new NotFoundException(nameof(Domain.Entities.Property), command.Id);

        var r = command.Request;

        if (r.Title is not null) p.Title = r.Title;
        if (r.ListingType is not null) p.ListingType = r.ListingType;
        if (r.ListingCategory is not null) p.ListingCategory = r.ListingCategory;
        if (r.PropertyType is not null) p.PropertyType = r.PropertyType;
        if (r.ConstructionStatus is not null) p.ConstructionStatus = r.ConstructionStatus;
        if (r.Facing is not null) p.Facing = r.Facing;
        if (r.Price is not null) p.Price = r.Price.Value;
        if (r.ProjectId is not null) p.ProjectId = r.ProjectId;
        if (r.Status is not null) p.Status = r.Status;
        if (r.ReraId is not null) p.ReraId = r.ReraId;
        if (r.AvailableFrom is not null) p.AvailableFrom = r.AvailableFrom;
        if (r.Address is not null) p.Address = r.Address;
        if (r.Locality is not null) p.Locality = r.Locality;
        if (r.City is not null) p.City = r.City;
        if (r.PinCode is not null) p.PinCode = r.PinCode;
        if (r.Landmark is not null) p.Landmark = r.Landmark;
        if (r.Latitude is not null) p.Latitude = r.Latitude;
        if (r.Longitude is not null) p.Longitude = r.Longitude;
        if (r.FloorNumber is not null) p.FloorNumber = r.FloorNumber;
        if (r.TotalFloors is not null) p.TotalFloors = r.TotalFloors;
        if (r.Currency is not null) p.Currency = r.Currency;
        if (r.MonthlyRent is not null) p.MonthlyRent = r.MonthlyRent;
        if (r.SecurityDeposit is not null) p.SecurityDeposit = r.SecurityDeposit;
        if (r.MaintenanceCharges is not null) p.MaintenanceCharges = r.MaintenanceCharges;
        if (r.PriceNegotiable is not null) p.PriceNegotiable = r.PriceNegotiable;
        if (r.CarpetArea is not null) p.CarpetArea = r.CarpetArea;
        if (r.BuiltupArea is not null) p.BuiltupArea = r.BuiltupArea;
        if (r.SuperBuiltupArea is not null) p.SuperBuiltupArea = r.SuperBuiltupArea;
        if (r.Bedrooms is not null) p.Bedrooms = r.Bedrooms;
        if (r.Bathrooms is not null) p.Bathrooms = r.Bathrooms;
        if (r.Balconies is not null) p.Balconies = r.Balconies;
        if (r.FurnishingStatus is not null) p.FurnishingStatus = r.FurnishingStatus;
        if (r.PropertyAge is not null) p.PropertyAge = r.PropertyAge;
        if (r.OwnershipType is not null) p.OwnershipType = r.OwnershipType;
        if (r.LoanAvailable is not null) p.LoanAvailable = r.LoanAvailable;
        if (r.VideoUrl is not null) p.VideoUrl = r.VideoUrl;
        if (r.ImageUrl is not null) p.ImageUrl = r.ImageUrl;
        if (r.Amenities is not null) p.Amenities = r.Amenities;
        if (r.InteriorFeatures is not null) p.InteriorFeatures = r.InteriorFeatures;
        if (r.Utilities is not null) p.Utilities = r.Utilities;
        if (r.IsPublished is not null) p.IsPublished = r.IsPublished.Value;
        if (r.IsFeatured is not null) p.IsFeatured = r.IsFeatured.Value;

        p.UpdatedAt = DateTime.UtcNow;

        await _propertyRepository.UpdateAsync(p, cancellationToken);

        return new PropertyResponse(
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
        );
    }
}
