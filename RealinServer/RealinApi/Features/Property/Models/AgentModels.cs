namespace RealinApi.Features.Property.Models;

public record CreateAgentRequest(
    Guid UserId,
    string? LicenseNumber = null,
    string? AgencyName = null,
    int? ExperienceYears = null
);

public record UpdateAgentRequest(
    string? LicenseNumber = null,
    string? AgencyName = null,
    int? ExperienceYears = null,
    decimal? Rating = null
);

public record AgentResponse(
    Guid Id,
    Guid UserId,
    string? LicenseNumber,
    string? AgencyName,
    int? ExperienceYears,
    decimal? Rating,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    UserSummary? User = null,
    int PropertyCount = 0,
    int BuilderCount = 0,
    int ProjectCount = 0
);
