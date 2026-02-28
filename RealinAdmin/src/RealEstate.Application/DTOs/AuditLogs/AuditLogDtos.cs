using RealEstate.Application.DTOs.Common;

namespace RealEstate.Application.DTOs.AuditLogs;

public record AuditLogResponse(
    Guid Id,
    string Action,
    string EntityType,
    Guid EntityId,
    Guid PerformedBy,
    string Details,
    string? IpAddress,
    DateTime CreatedAt,
    UserSummary? PerformedByUser = null
);

public record AuditLogFilter(
    string? EntityType = null,
    Guid? EntityId = null,
    Guid? PerformedBy = null,
    DateTime? From = null,
    DateTime? To = null
);
