using System;

namespace RealinApi.Data.Entities;

/// <summary>
/// Defines the hierarchical role types with explicit integer values
/// Higher number = higher privilege level
/// </summary>
public enum RoleType
{
    Guest = 0,        // Unverified or limited access users
    User = 1,         // Standard verified users
    Agent = 5,        // Real estate agents with property management access
    Admin = 10,       // System administrators
    SuperAdmin = 100  // Full system access
}

/// <summary>
/// Represents a role in the system with associated permissions
/// </summary>
public class Role
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The RoleType enum value (e.g., SuperAdmin = 100)
    /// This serves as both the identifier and hierarchy indicator
    /// </summary>
    public required RoleType RoleType { get; set; }
    
    /// <summary>
    /// Human-readable name (e.g., "Super Administrator")
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Description of the role's purpose and permissions
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Flag to enable/disable role without deletion
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Defines application modules/features that can have permissions
/// </summary>
public class Module
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Unique code for the module (e.g., "PROPERTY", "USER_MGMT", "ANALYTICS")
    /// </summary>
    public required string Code { get; set; }
    
    /// <summary>
    /// Display name for the module
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Description of what this module controls
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Flag to enable/disable module
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Junction table defining what permissions a role has on specific modules
/// Provides granular CRUD access control
/// </summary>
public class RolePermission
{
    public Guid Id { get; set; }
    
    public required Guid RoleId { get; set; }
    public required Guid ModuleId { get; set; }
    
    /// <summary>
    /// Can view/read data in this module
    /// </summary>
    public bool CanRead { get; set; } = false;
    
    /// <summary>
    /// Can create new records in this module
    /// </summary>
    public bool CanCreate { get; set; } = false;
    
    /// <summary>
    /// Can update existing records in this module
    /// </summary>
    public bool CanUpdate { get; set; } = false;
    
    /// <summary>
    /// Can delete records in this module
    /// </summary>
    public bool CanDelete { get; set; } = false;
    
    /// <summary>
    /// Special administrative privileges (e.g., approve, publish, manage settings)
    /// </summary>
    public bool CanManage { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Module Module { get; set; } = null!;
}
