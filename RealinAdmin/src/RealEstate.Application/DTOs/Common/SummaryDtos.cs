namespace RealEstate.Application.DTOs.Common;

public record AgentSummary(Guid Id, string? AgencyName, decimal? Rating);
public record ProjectSummary(Guid Id, string Name, string? City, Guid BuilderId);
public record BuilderSummary(Guid Id, string Name);
public record UserSummary(Guid Id, string? Name, string? Email, string? PhoneNumber = null);
