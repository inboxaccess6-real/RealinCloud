using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;

namespace RealinApi.Infrastructure.Authorization;

/// <summary>
/// Service for checking user permissions based on roles and modules
/// </summary>
public interface IPermissionService
{
    Task<bool> CanReadAsync(Guid userId, string moduleCode);
    Task<bool> CanCreateAsync(Guid userId, string moduleCode);
    Task<bool> CanUpdateAsync(Guid userId, string moduleCode);
    Task<bool> CanDeleteAsync(Guid userId, string moduleCode);
    Task<bool> CanManageAsync(Guid userId, string moduleCode);
    Task<bool> HasRoleAsync(Guid userId, RoleType roleType);
    Task<bool> HasMinimumRoleAsync(Guid userId, RoleType minimumRoleType);
    Task<RolePermission?> GetPermissionsAsync(Guid userId, string moduleCode);
}

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(AppDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CanReadAsync(Guid userId, string moduleCode)
    {
        var permission = await GetPermissionsAsync(userId, moduleCode);
        return permission?.CanRead ?? false;
    }

    public async Task<bool> CanCreateAsync(Guid userId, string moduleCode)
    {
        var permission = await GetPermissionsAsync(userId, moduleCode);
        return permission?.CanCreate ?? false;
    }

    public async Task<bool> CanUpdateAsync(Guid userId, string moduleCode)
    {
        var permission = await GetPermissionsAsync(userId, moduleCode);
        return permission?.CanUpdate ?? false;
    }

    public async Task<bool> CanDeleteAsync(Guid userId, string moduleCode)
    {
        var permission = await GetPermissionsAsync(userId, moduleCode);
        return permission?.CanDelete ?? false;
    }

    public async Task<bool> CanManageAsync(Guid userId, string moduleCode)
    {
        var permission = await GetPermissionsAsync(userId, moduleCode);
        return permission?.CanManage ?? false;
    }

    public async Task<bool> HasRoleAsync(Guid userId, RoleType roleType)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        return user?.Role?.RoleType == roleType;
    }

    /// <summary>
    /// Checks if user has at least the specified role level
    /// Example: HasMinimumRoleAsync(userId, RoleType.Admin) returns true for Admin and SuperAdmin
    /// </summary>
    public async Task<bool> HasMinimumRoleAsync(Guid userId, RoleType minimumRoleType)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user?.Role == null) return false;

        // Compare enum integer values (SuperAdmin = 100, Admin = 10, etc.)
        return (int)user.Role.RoleType >= (int)minimumRoleType;
    }

    public async Task<RolePermission?> GetPermissionsAsync(Guid userId, string moduleCode)
    {
        var permission = await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.Module)
            .SelectMany(u => u.Role.Permissions)
            .FirstOrDefaultAsync(p => p.Module.Code == moduleCode && p.Module.IsActive);

        return permission;
    }
}

/// <summary>
/// Extension methods for permission checking in controllers
/// </summary>
public static class PermissionExtensions
{
    /// <summary>
    /// Checks if the current user has permission to perform an action
    /// Usage: await HttpContext.CheckPermissionAsync(permissionService, "PROPERTY", p => p.CanCreate)
    /// </summary>
    public static async Task<bool> CheckPermissionAsync(
        this HttpContext httpContext,
        IPermissionService permissionService,
        string moduleCode,
        Func<RolePermission, bool> permissionSelector)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value 
                          ?? httpContext.User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        var permissions = await permissionService.GetPermissionsAsync(userId, moduleCode);
        return permissions != null && permissionSelector(permissions);
    }

    /// <summary>
    /// Gets the current user's ID from JWT claims
    /// </summary>
    public static Guid? GetUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value 
                          ?? httpContext.User.FindFirst("userId")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
