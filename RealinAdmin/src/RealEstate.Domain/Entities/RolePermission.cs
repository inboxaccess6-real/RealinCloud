namespace RealEstate.Domain.Entities;

public class RolePermission
{
    public Guid Id { get; set; }
    public required Guid RoleId { get; set; }
    public required Guid ModuleId { get; set; }

    public bool CanRead { get; set; } = false;
    public bool CanCreate { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanManage { get; set; } = false;
    public bool CanExport { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    public Module Module { get; set; } = null!;
}
