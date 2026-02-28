namespace RealinApi.Data.Entities;

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

    /// <summary>
    /// Can export data from this module (CSV, Excel)
    /// </summary>
    public bool CanExport { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Module Module { get; set; } = null!;
}
