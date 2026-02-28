namespace RealEstate.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public required RoleType RoleType { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}
