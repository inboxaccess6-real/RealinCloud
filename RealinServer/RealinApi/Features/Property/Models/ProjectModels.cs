namespace RealinApi.Features.Property.Models;

public record CreateProjectRequest(
    string Name,
    Guid BuilderId,
    Guid? CreatedByAgentId = null,
    string? ReraId = null,
    string? Address = null,
    string? Locality = null,
    string? City = null,
    string? PinCode = null,
    string? Landmark = null,
    double? Latitude = null,
    double? Longitude = null,
    string? ConstructionStatus = null,
    DateTime? LaunchDate = null,
    DateTime? PossessionDate = null,
    string? Status = null,
    int? TotalTowers = null,
    int? TotalUnits = null
);

public record UpdateProjectRequest(
    string? Name = null,
    string? ReraId = null,
    string? Address = null,
    string? Locality = null,
    string? City = null,
    string? PinCode = null,
    string? Landmark = null,
    double? Latitude = null,
    double? Longitude = null,
    string? ConstructionStatus = null,
    DateTime? LaunchDate = null,
    DateTime? PossessionDate = null,
    string? Status = null,
    int? TotalTowers = null,
    int? TotalUnits = null
);

public record ProjectResponse(
    Guid Id,
    string Name,
    Guid BuilderId,
    string? ReraId,
    string? Address,
    string? Locality,
    string? City,
    string? PinCode,
    string? Landmark,
    double? Latitude,
    double? Longitude,
    string? ConstructionStatus,
    DateTime? LaunchDate,
    DateTime? PossessionDate,
    string? Status,
    int? TotalTowers,
    int? TotalUnits,
    DateTime CreatedAt,
    Guid? CreatedByAgentId,
    BuilderSummary? Builder = null,
    AgentSummary? CreatedByAgent = null,
    int PropertyCount = 0
);
