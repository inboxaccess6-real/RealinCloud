namespace RealinApi.Data.Entities;

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

