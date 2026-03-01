using RealEstate.Domain.Entities;

namespace RealEstate.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string? Email,
    string? PhoneNumber,
    string? Name,
    AuthProvider LoginType,
    Guid RoleId,
    string RoleName,
    bool IsActive,
    bool IsBlocked,
    bool IsBlacklisted,
    string? BlacklistReason,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UpdateUserRequest(
    string? Name = null,
    string? Email = null,
    string? PhoneNumber = null,
    Guid? RoleId = null,
    bool? IsActive = null
);
