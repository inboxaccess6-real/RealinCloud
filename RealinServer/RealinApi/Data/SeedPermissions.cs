using Microsoft.EntityFrameworkCore;
using RealinApi.Data.Entities;

namespace RealinApi.Data;

/// <summary>
/// Seeds default role permissions for the application.
/// Run this after initial migration to set up permissions.
/// </summary>
public static class SeedPermissions
{
    public static async Task SeedDefaultPermissionsAsync(AppDbContext context)
    {
        // Check if permissions already exist
        if (await context.RolePermissions.AnyAsync())
        {
            Console.WriteLine("Permissions already seeded. Skipping...");
            return;
        }

        var roles = await context.Roles.ToDictionaryAsync(r => r.RoleType, r => r.Id);
        var modules = await context.Modules.ToDictionaryAsync(m => m.Code, m => m.Id);

        var permissions = new List<RolePermission>();

        // ========================================
        // GUEST ROLE (0) - Very limited access
        // ========================================
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Guest],
            ModuleId = modules["PROPERTY_BROWSE"],
            CanRead = true,    // Can view properties
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false,
            CanManage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ========================================
        // USER ROLE (1) - Standard buyers/renters
        // ========================================
        
        // Browse properties
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.User],
            ModuleId = modules["PROPERTY_BROWSE"],
            CanRead = true,    // View properties
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false,
            CanManage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Manage own bookmarks
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.User],
            ModuleId = modules["BOOKMARKS"],
            CanRead = true,    // View own bookmarks
            CanCreate = true,  // Add bookmarks
            CanUpdate = true,  // Update bookmark notes
            CanDelete = true,  // Remove bookmarks
            CanManage = false, // Cannot manage others' bookmarks
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ========================================
        // AGENT ROLE (5) - Real estate agents
        // ========================================
        
        // Browse all properties
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Agent],
            ModuleId = modules["PROPERTY_BROWSE"],
            CanRead = true,
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false,
            CanManage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Manage properties (CREATE, UPDATE own properties)
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Agent],
            ModuleId = modules["PROPERTY_MANAGE"],
            CanRead = true,    // View all property details
            CanCreate = true,  // Create new listings
            CanUpdate = true,  // Update own listings
            CanDelete = false, // Cannot delete (admins only)
            CanManage = false, // Cannot approve/publish others' listings
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Agent bookmarks
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Agent],
            ModuleId = modules["BOOKMARKS"],
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,
            CanManage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // View analytics for own properties
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Agent],
            ModuleId = modules["ANALYTICS"],
            CanRead = true,    // View own property analytics
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false,
            CanManage = false, // Cannot manage analytics settings
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ========================================
        // ADMIN ROLE (10) - System administrators
        // ========================================
        
        // Full property browsing
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Admin],
            ModuleId = modules["PROPERTY_BROWSE"],
            CanRead = true,
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false,
            CanManage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Full property management
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Admin],
            ModuleId = modules["PROPERTY_MANAGE"],
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,   // Can delete properties
            CanManage = true,   // Can approve/reject listings
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // User management
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Admin],
            ModuleId = modules["USER_MGMT"],
            CanRead = true,
            CanCreate = true,   // Can create users
            CanUpdate = true,   // Can modify users
            CanDelete = false,  // Cannot delete users (SuperAdmin only)
            CanManage = true,   // Can change roles (except SuperAdmin)
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Full analytics
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Admin],
            ModuleId = modules["ANALYTICS"],
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true,
            CanManage = true,   // Configure analytics
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // System settings
        permissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roles[RoleType.Admin],
            ModuleId = modules["SETTINGS"],
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = false,
            CanManage = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ========================================
        // SUPERADMIN ROLE (100) - Full access
        // ========================================
        
        // Grant full permissions to all modules for SuperAdmin
        foreach (var moduleCode in modules.Keys)
        {
            permissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roles[RoleType.SuperAdmin],
                ModuleId = modules[moduleCode],
                CanRead = true,
                CanCreate = true,
                CanUpdate = true,
                CanDelete = true,
                CanManage = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Add all permissions
        await context.RolePermissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Successfully seeded {permissions.Count} default permissions!");
        Console.WriteLine("\nPermission Summary:");
        Console.WriteLine("- Guest: Browse properties only");
        Console.WriteLine("- User: Browse properties + manage own bookmarks");
        Console.WriteLine("- Agent: Create/edit properties + analytics");
        Console.WriteLine("- Admin: Full property & user management");
        Console.WriteLine("- SuperAdmin: Full system access");
    }
}
