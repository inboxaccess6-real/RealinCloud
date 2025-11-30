namespace RealinApi.Data.Entities;

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
