using Microsoft.EntityFrameworkCore;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedDefaultPermissionsAsync(AppDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // Module IDs
        var userMgmt = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var propertyBrowse = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var propertyManage = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var bookmarks = Guid.Parse("10000000-0000-0000-0000-000000000004");
        var analytics = Guid.Parse("10000000-0000-0000-0000-000000000005");
        var settings = Guid.Parse("10000000-0000-0000-0000-000000000006");
        var blacklist = Guid.Parse("10000000-0000-0000-0000-000000000007");
        var auditLogs = Guid.Parse("10000000-0000-0000-0000-000000000008");
        var reports = Guid.Parse("10000000-0000-0000-0000-000000000009");
        var approvals = Guid.Parse("10000000-0000-0000-0000-00000000000a");
        var agentMgmt = Guid.Parse("10000000-0000-0000-0000-00000000000b");
        var roleMgmt = Guid.Parse("10000000-0000-0000-0000-00000000000c");

        // Role IDs
        var guestId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var agentId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var adminId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        var superAdminId = Guid.Parse("00000000-0000-0000-0000-000000000005");

        var allModules = new[]
        {
            userMgmt, propertyBrowse, propertyManage, bookmarks, analytics,
            settings, blacklist, auditLogs, reports, approvals, agentMgmt, roleMgmt
        };

        var permissions = new List<RolePermission>();

        // Guest: PROPERTY_BROWSE (read only)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = guestId,
            ModuleId = propertyBrowse,
            CanRead = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // User: PROPERTY_BROWSE (read), BOOKMARKS (CRUD)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = userId,
            ModuleId = propertyBrowse,
            CanRead = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = userId,
            ModuleId = bookmarks,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Agent: PROPERTY_BROWSE (read), PROPERTY_MANAGE (read+create+update), BOOKMARKS (CRUD), ANALYTICS (read)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = agentId,
            ModuleId = propertyBrowse,
            CanRead = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = agentId,
            ModuleId = propertyManage,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = agentId,
            ModuleId = bookmarks,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = agentId,
            ModuleId = analytics,
            CanRead = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Admin: PROPERTY_BROWSE (read), PROPERTY_MANAGE (full+manage), USER_MGMT (read+create+update+manage),
        //        ANALYTICS (full+manage), SETTINGS (read+create+update+manage)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = propertyBrowse,
            CanRead = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = propertyManage,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,
            CanManage = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = userMgmt,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanManage = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = analytics,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,
            CanManage = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = settings,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanManage = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Admin: AGENT_MGMT (read+create+update+manage)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = agentMgmt,
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanManage = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Admin: ROLE_MGMT (read+update+manage)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = adminId,
            ModuleId = roleMgmt,
            CanRead = true,
            CanUpdate = true,
            CanManage = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // SuperAdmin: Full permissions on ALL modules
        foreach (var moduleId in allModules)
        {
            permissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = superAdminId,
                ModuleId = moduleId,
                CanRead = true,
                CanCreate = true,
                CanUpdate = true,
                CanDelete = true,
                CanManage = true,
                CanExport = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await context.RolePermissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
}
