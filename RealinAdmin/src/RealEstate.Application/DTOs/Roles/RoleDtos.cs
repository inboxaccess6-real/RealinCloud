namespace RealEstate.Application.DTOs.Roles;

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int UserCount,
    List<RolePermissionDto>? Permissions = null
);

public record RolePermissionDto(
    Guid ModuleId,
    string ModuleCode,
    string ModuleName,
    bool CanRead,
    bool CanCreate,
    bool CanUpdate,
    bool CanDelete,
    bool CanManage,
    bool CanExport
);

public record CreateRoleRequest(
    string Name,
    string? Description = null,
    bool IsActive = true
);

public record UpdateRoleRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null
);

public record AssignPermissionsRequest(
    List<PermissionAssignment> Permissions
);

public record PermissionAssignment(
    Guid ModuleId,
    bool CanRead = false,
    bool CanCreate = false,
    bool CanUpdate = false,
    bool CanDelete = false,
    bool CanManage = false,
    bool CanExport = false
);
