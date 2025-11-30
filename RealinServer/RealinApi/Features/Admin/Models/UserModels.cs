using RealinApi.Data.Entities;

namespace RealinApi.Features.Admin.Models;

public record UserResponse(
    Guid Id,
    string Email,
    string? PhoneNumber,
    string? Name,
    AuthProvider Provider,
    string? OAuthProviderId,
    Guid RoleId,
    string RoleName,
    RoleType RoleType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UpdateUserRequest(
    string? Name = null,
    string? PhoneNumber = null,
    Guid? RoleId = null,
    bool? IsActive = null
);
