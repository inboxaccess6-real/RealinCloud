namespace RealinApi.Features.Property.Models;

public record CreateBuilderRequest(
    string Name,
    Guid? CreatedByAgentId = null,
    string? Email = null,
    string? Phone = null,
    int? EstablishedYear = null,
    string? RegistrationNumber = null,
    string? HeadquartersAddress = null,
    string? Website = null,
    bool Active = true
);

public record UpdateBuilderRequest(
    string? Name = null,
    string? Email = null,
    string? Phone = null,
    int? EstablishedYear = null,
    string? RegistrationNumber = null,
    string? HeadquartersAddress = null,
    string? Website = null,
    bool? Active = null
);

public record BuilderResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    int? EstablishedYear,
    string? RegistrationNumber,
    string? HeadquartersAddress,
    string? Website,
    bool Active,
    DateTime CreatedAt,
    Guid? CreatedByAgentId,
    AgentSummary? CreatedByAgent = null,
    int ProjectCount = 0
);
