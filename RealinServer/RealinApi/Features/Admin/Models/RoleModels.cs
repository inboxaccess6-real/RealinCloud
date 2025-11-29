using RealinApi.Data.Entities;

namespace RealinApi.Features.Admin.Models;

public record RoleResponse(
    Guid Id,
    RoleType RoleType,
    int RoleValue,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int UserCount
);

public record CreateRoleRequest(
    RoleType RoleType,
    string Name,
    string? Description = null,
    bool IsActive = true
);

public record UpdateRoleRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null
);
