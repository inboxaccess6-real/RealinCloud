using MediatR;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Properties.Commands;

public record CreatePropertyCommand(CreatePropertyRequest Request) : IRequest<PropertyResponse>;

public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, PropertyResponse>
{
    private readonly IPropertyRepository _propertyRepository;

    public CreatePropertyHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyResponse> Handle(CreatePropertyCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        var now = DateTime.UtcNow;

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = r.Title,
            ListingType = r.ListingType,
            ListingCategory = r.ListingCategory,
            PropertyType = r.PropertyType,
            ConstructionStatus = r.ConstructionStatus,
            Facing = r.Facing,
            Price = r.Price,
            AgentId = r.AgentId,
            ProjectId = r.ProjectId,
            ReraId = r.ReraId,
            AvailableFrom = r.AvailableFrom,
            Address = r.Address,
            Locality = r.Locality,
            City = r.City,
            PinCode = r.PinCode,
            Landmark = r.Landmark,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            FloorNumber = r.FloorNumber,
            TotalFloors = r.TotalFloors,
            Currency = r.Currency,
            MonthlyRent = r.MonthlyRent,
            SecurityDeposit = r.SecurityDeposit,
            MaintenanceCharges = r.MaintenanceCharges,
            PriceNegotiable = r.PriceNegotiable,
            CarpetArea = r.CarpetArea,
            BuiltupArea = r.BuiltupArea,
            SuperBuiltupArea = r.SuperBuiltupArea,
            Bedrooms = r.Bedrooms,
            Bathrooms = r.Bathrooms,
            Balconies = r.Balconies,
            FurnishingStatus = r.FurnishingStatus,
            PropertyAge = r.PropertyAge,
            OwnershipType = r.OwnershipType,
            LoanAvailable = r.LoanAvailable,
            VideoUrl = r.VideoUrl,
            ImageUrl = r.ImageUrl,
            Amenities = r.Amenities ?? "{}",
            InteriorFeatures = r.InteriorFeatures ?? "{}",
            Utilities = r.Utilities ?? "{}",
            IsPublished = r.IsPublished,
            IsFeatured = r.IsFeatured,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _propertyRepository.AddAsync(property, cancellationToken);

        return new PropertyResponse(
            property.Id, property.Title, property.ListingType, property.ListingCategory,
            property.PropertyType, property.ConstructionStatus, property.Facing,
            property.Price, property.Status, property.AgentId, property.ProjectId,
            property.ReraId, property.AvailableFrom,
            property.Address, property.Locality, property.City, property.PinCode, property.Landmark,
            property.Latitude, property.Longitude,
            property.FloorNumber, property.TotalFloors,
            property.Currency, property.MonthlyRent, property.SecurityDeposit, property.MaintenanceCharges,
            property.PriceNegotiable,
            property.CarpetArea, property.BuiltupArea, property.SuperBuiltupArea,
            property.Bedrooms, property.Bathrooms, property.Balconies,
            property.FurnishingStatus, property.PropertyAge,
            property.OwnershipType, property.LoanAvailable,
            property.VideoUrl, property.ImageUrl,
            property.Amenities, property.InteriorFeatures, property.Utilities,
            property.IsPublished, property.IsFeatured,
            property.ApprovalStatus, property.RejectionReason,
            property.IsFlagged, property.FlagReason,
            property.IsDeleted,
            property.CreatedAt, property.UpdatedAt
        );
    }
}
