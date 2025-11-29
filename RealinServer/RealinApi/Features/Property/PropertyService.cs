using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public interface IPropertyService
{
    Task<PropertyResponse?> GetPropertyByIdAsync(Guid id);
    Task<List<PropertyResponse>> GetPropertiesAsync(int skip = 0, int take = 50, string? city = null, bool? isPublished = null);
    Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request);
    Task<PropertyResponse?> UpdatePropertyAsync(Guid id, UpdatePropertyRequest request);
    Task<bool> DeletePropertyAsync(Guid id);
}

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(AppDbContext context, ILogger<PropertyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PropertyResponse?> GetPropertyByIdAsync(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.Agent)
                .ThenInclude(a => a.User)
            .Include(p => p.Project)
                .ThenInclude(p => p!.Builder)
            .FirstOrDefaultAsync(p => p.Id == id);

        return property == null ? null : MapToPropertyResponse(property);
    }

    public async Task<List<PropertyResponse>> GetPropertiesAsync(int skip = 0, int take = 50, string? city = null, bool? isPublished = null)
    {
        var query = _context.Properties
            .Include(p => p.Agent)
            .Include(p => p.Project)
            .AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(p => p.City == city);

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var properties = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return properties.Select(MapToPropertyResponse).ToList();
    }

    public async Task<PropertyResponse> CreatePropertyAsync(CreatePropertyRequest request)
    {
        // Verify agent exists
        var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.AgentId);
        if (!agentExists)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        // Verify project exists if provided
        if (request.ProjectId.HasValue)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == request.ProjectId.Value);
            if (!projectExists)
                throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");
        }

        var property = new Data.Entities.Property
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            ListingType = request.ListingType,
            ListingCategory = request.ListingCategory,
            PropertyType = request.PropertyType,
            ConstructionStatus = request.ConstructionStatus,
            Facing = request.Facing,
            Price = request.Price,
            AgentId = request.AgentId,
            ProjectId = request.ProjectId,
            ReraId = request.ReraId,
            AvailableFrom = request.AvailableFrom,
            Address = request.Address,
            Locality = request.Locality,
            City = request.City,
            PinCode = request.PinCode,
            Landmark = request.Landmark,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            FloorNumber = request.FloorNumber,
            TotalFloors = request.TotalFloors,
            Currency = request.Currency,
            MonthlyRent = request.MonthlyRent,
            SecurityDeposit = request.SecurityDeposit,
            MaintenanceCharges = request.MaintenanceCharges,
            PriceNegotiable = request.PriceNegotiable,
            CarpetArea = request.CarpetArea,
            BuiltupArea = request.BuiltupArea,
            SuperBuiltupArea = request.SuperBuiltupArea,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            Balconies = request.Balconies,
            FurnishingStatus = request.FurnishingStatus,
            PropertyAge = request.PropertyAge,
            OwnershipType = request.OwnershipType,
            LoanAvailable = request.LoanAvailable,
            VideoUrl = request.VideoUrl,
            ImageUrl = request.ImageUrl,
            Amenities = request.Amenities ?? "{}",
            InteriorFeatures = request.InteriorFeatures ?? "{}",
            Utilities = request.Utilities ?? "{}",
            IsPublished = request.IsPublished,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        return (await GetPropertyByIdAsync(property.Id))!;
    }

    public async Task<PropertyResponse?> UpdatePropertyAsync(Guid id, UpdatePropertyRequest request)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return null;

        // Update only provided fields
        if (request.Title != null) property.Title = request.Title;
        if (request.ListingType != null) property.ListingType = request.ListingType;
        if (request.ListingCategory != null) property.ListingCategory = request.ListingCategory;
        if (request.PropertyType != null) property.PropertyType = request.PropertyType;
        if (request.ConstructionStatus != null) property.ConstructionStatus = request.ConstructionStatus;
        if (request.Facing != null) property.Facing = request.Facing;
        if (request.Price.HasValue) property.Price = request.Price.Value;
        if (request.ProjectId.HasValue) property.ProjectId = request.ProjectId.Value;
        if (request.Status != null) property.Status = request.Status;
        if (request.ReraId != null) property.ReraId = request.ReraId;
        if (request.AvailableFrom.HasValue) property.AvailableFrom = request.AvailableFrom;
        if (request.Address != null) property.Address = request.Address;
        if (request.Locality != null) property.Locality = request.Locality;
        if (request.City != null) property.City = request.City;
        if (request.PinCode != null) property.PinCode = request.PinCode;
        if (request.Landmark != null) property.Landmark = request.Landmark;
        if (request.Latitude.HasValue) property.Latitude = request.Latitude;
        if (request.Longitude.HasValue) property.Longitude = request.Longitude;
        if (request.FloorNumber.HasValue) property.FloorNumber = request.FloorNumber;
        if (request.TotalFloors.HasValue) property.TotalFloors = request.TotalFloors;
        if (request.Currency != null) property.Currency = request.Currency;
        if (request.MonthlyRent.HasValue) property.MonthlyRent = request.MonthlyRent;
        if (request.SecurityDeposit.HasValue) property.SecurityDeposit = request.SecurityDeposit;
        if (request.MaintenanceCharges.HasValue) property.MaintenanceCharges = request.MaintenanceCharges;
        if (request.PriceNegotiable.HasValue) property.PriceNegotiable = request.PriceNegotiable;
        if (request.CarpetArea.HasValue) property.CarpetArea = request.CarpetArea;
        if (request.BuiltupArea.HasValue) property.BuiltupArea = request.BuiltupArea;
        if (request.SuperBuiltupArea.HasValue) property.SuperBuiltupArea = request.SuperBuiltupArea;
        if (request.Bedrooms.HasValue) property.Bedrooms = request.Bedrooms;
        if (request.Bathrooms.HasValue) property.Bathrooms = request.Bathrooms;
        if (request.Balconies.HasValue) property.Balconies = request.Balconies;
        if (request.FurnishingStatus != null) property.FurnishingStatus = request.FurnishingStatus;
        if (request.PropertyAge.HasValue) property.PropertyAge = request.PropertyAge;
        if (request.OwnershipType != null) property.OwnershipType = request.OwnershipType;
        if (request.LoanAvailable.HasValue) property.LoanAvailable = request.LoanAvailable;
        if (request.VideoUrl != null) property.VideoUrl = request.VideoUrl;
        if (request.ImageUrl != null) property.ImageUrl = request.ImageUrl;
        if (request.Amenities != null) property.Amenities = request.Amenities;
        if (request.InteriorFeatures != null) property.InteriorFeatures = request.InteriorFeatures;
        if (request.Utilities != null) property.Utilities = request.Utilities;
        if (request.IsPublished.HasValue) property.IsPublished = request.IsPublished.Value;
        if (request.IsFeatured.HasValue) property.IsFeatured = request.IsFeatured.Value;

        property.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetPropertyByIdAsync(id);
    }

    public async Task<bool> DeletePropertyAsync(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return false;

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        return true;
    }

    private static PropertyResponse MapToPropertyResponse(Data.Entities.Property property)
    {
        return new PropertyResponse(
            property.Id,
            property.Title,
            property.ListingType,
            property.ListingCategory,
            property.PropertyType,
            property.ConstructionStatus,
            property.Facing,
            property.Price,
            property.Status,
            property.AgentId,
            property.ProjectId,
            property.ReraId,
            property.AvailableFrom,
            property.Address,
            property.Locality,
            property.City,
            property.PinCode,
            property.Landmark,
            property.Latitude,
            property.Longitude,
            property.FloorNumber,
            property.TotalFloors,
            property.Currency,
            property.MonthlyRent,
            property.SecurityDeposit,
            property.MaintenanceCharges,
            property.PriceNegotiable,
            property.CarpetArea,
            property.BuiltupArea,
            property.SuperBuiltupArea,
            property.Bedrooms,
            property.Bathrooms,
            property.Balconies,
            property.FurnishingStatus,
            property.PropertyAge,
            property.OwnershipType,
            property.LoanAvailable,
            property.VideoUrl,
            property.ImageUrl,
            property.Amenities,
            property.InteriorFeatures,
            property.Utilities,
            property.IsPublished,
            property.IsFeatured,
            property.CreatedAt,
            property.UpdatedAt,
            property.Agent == null ? null : new AgentSummary(property.Agent.Id, property.Agent.AgencyName, property.Agent.Rating),
            property.Project == null ? null : new ProjectSummary(property.Project.Id, property.Project.Name, property.Project.City, property.Project.BuilderId)
        );
    }
}
