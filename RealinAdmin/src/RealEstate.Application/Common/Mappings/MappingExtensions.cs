using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.AuditLogs;
using RealEstate.Application.DTOs.Builders;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Projects;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.DTOs.Users;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Common.Mappings;

public static class MappingExtensions
{
    public static UserResponse ToResponse(this User user) => new(
        user.Id, user.Email, user.PhoneNumber, user.Name,
        user.Provider, user.RoleId, user.Role?.Name ?? "",
        user.IsActive, user.IsBlocked, user.IsBlacklisted,
        user.BlacklistReason, user.IsDeleted,
        user.CreatedAt, user.UpdatedAt
    );

    public static AgentResponse ToResponse(this Agent agent, int propertyCount = 0, int builderCount = 0, int projectCount = 0) => new(
        agent.Id, agent.UserId,
        agent.LicenseNumber, agent.AgencyName,
        agent.ExperienceYears, agent.Rating,
        agent.Status, agent.VerificationNotes,
        agent.IsBlocked, agent.IsBlacklisted, agent.IsDeleted,
        agent.CreatedAt, agent.UpdatedAt,
        agent.User == null ? null : new UserSummary(agent.User.Id, agent.User.Name, agent.User.Email),
        propertyCount, builderCount, projectCount
    );

    public static PropertyResponse ToResponse(this Property property) => new(
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
        property.CreatedAt, property.UpdatedAt,
        property.Agent == null ? null : new AgentSummary(property.Agent.Id, property.Agent.AgencyName, property.Agent.Rating),
        property.Project == null ? null : new ProjectSummary(property.Project.Id, property.Project.Name, property.Project.City, property.Project.BuilderId)
    );

    public static BuilderResponse ToResponse(this Builder builder, int projectCount = 0) => new(
        builder.Id, builder.Name,
        builder.Email, builder.Phone,
        builder.EstablishedYear, builder.RegistrationNumber,
        builder.HeadquartersAddress, builder.Website,
        builder.Active,
        builder.IsBlocked, builder.IsBlacklisted, builder.IsDeleted,
        builder.CreatedAt, builder.UpdatedAt,
        builder.CreatedByAgentId,
        builder.CreatedByAgent == null ? null : new AgentSummary(builder.CreatedByAgent.Id, builder.CreatedByAgent.AgencyName, builder.CreatedByAgent.Rating),
        projectCount
    );

    public static ProjectResponse ToResponse(this Project project, int propertyCount = 0) => new(
        project.Id, project.Name, project.BuilderId,
        project.ReraId,
        project.Address, project.Locality, project.City, project.PinCode, project.Landmark,
        project.Latitude, project.Longitude,
        project.ConstructionStatus,
        project.LaunchDate, project.PossessionDate,
        project.Status, project.TotalTowers, project.TotalUnits,
        project.IsBlocked, project.IsBlacklisted, project.IsDeleted,
        project.CreatedAt, project.UpdatedAt,
        project.CreatedByAgentId,
        project.Builder == null ? null : new BuilderSummary(project.Builder.Id, project.Builder.Name),
        project.CreatedByAgent == null ? null : new AgentSummary(project.CreatedByAgent.Id, project.CreatedByAgent.AgencyName, project.CreatedByAgent.Rating),
        propertyCount
    );

    public static AuditLogResponse ToResponse(this AuditLog log) => new(
        log.Id, log.Action, log.EntityType, log.EntityId,
        log.PerformedBy, log.Details, log.IpAddress, log.CreatedAt,
        log.PerformedByUser == null ? null : new UserSummary(log.PerformedByUser.Id, log.PerformedByUser.Name, log.PerformedByUser.Email)
    );
}
