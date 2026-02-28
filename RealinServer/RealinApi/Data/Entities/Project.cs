namespace RealinApi.Data.Entities;

/// <summary>
/// Represents a real estate project by a builder
/// </summary>
public class Project
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? ReraId { get; set; }
    public required Guid BuilderId { get; set; }
    
    // Embedded address
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? City { get; set; }
    public string? PinCode { get; set; }
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Construction details
    public string? ConstructionStatus { get; set; } // ready / under_construction / new_launch
    public DateTime? LaunchDate { get; set; }
    public DateTime? PossessionDate { get; set; }
    public string? Status { get; set; } // active / completed / on_hold
    public int? TotalTowers { get; set; }
    public int? TotalUnits { get; set; }
    
    // Moderation
    public bool IsBlocked { get; set; } = false;
    public bool IsBlacklisted { get; set; } = false;
    public string? BlacklistReason { get; set; }
    public Guid? BlockedBy { get; set; }
    public DateTime? BlockedAt { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Audit - track which agent created this project
    public Guid? CreatedByAgentId { get; set; }
    
    // Navigation properties
    public Builder Builder { get; set; } = null!;
    public Agent? CreatedByAgent { get; set; }
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
