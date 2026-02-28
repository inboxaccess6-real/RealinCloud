using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string moduleCode, string permission)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
                    .ThenInclude(p => p.Module)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return false;

        var rolePermission = user.Role.Permissions
            .FirstOrDefault(p => p.Module.Code == moduleCode);

        if (rolePermission is null)
            return false;

        return permission.ToLowerInvariant() switch
        {
            "read" => rolePermission.CanRead,
            "create" => rolePermission.CanCreate,
            "update" => rolePermission.CanUpdate,
            "delete" => rolePermission.CanDelete,
            "manage" => rolePermission.CanManage,
            "export" => rolePermission.CanExport,
            _ => false
        };
    }

    public async Task<Dictionary<string, HashSet<string>>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
                    .ThenInclude(p => p.Module)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var result = new Dictionary<string, HashSet<string>>();

        if (user is null)
            return result;

        foreach (var rolePermission in user.Role.Permissions)
        {
            var permissions = new HashSet<string>();

            if (rolePermission.CanRead) permissions.Add("Read");
            if (rolePermission.CanCreate) permissions.Add("Create");
            if (rolePermission.CanUpdate) permissions.Add("Update");
            if (rolePermission.CanDelete) permissions.Add("Delete");
            if (rolePermission.CanManage) permissions.Add("Manage");
            if (rolePermission.CanExport) permissions.Add("Export");

            if (permissions.Count > 0)
            {
                result[rolePermission.Module.Code] = permissions;
            }
        }

        return result;
    }
}
