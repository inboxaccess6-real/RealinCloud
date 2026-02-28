namespace RealEstate.Application.DTOs.Modules;

public record ModuleResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int PermissionCount
);

public record CreateModuleRequest(
    string Code,
    string Name,
    string? Description = null,
    bool IsActive = true
);

public record UpdateModuleRequest(
    string? Code = null,
    string? Name = null,
    string? Description = null,
    bool? IsActive = null
);
