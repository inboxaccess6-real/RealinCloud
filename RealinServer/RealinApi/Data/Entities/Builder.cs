namespace RealinApi.Data.Entities;

/// <summary>
/// Represents a real estate builder/developer
/// </summary>
public class Builder
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? EstablishedYear { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? HeadquartersAddress { get; set; }
    public string? Website { get; set; }
    public bool Active { get; set; } = true;

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

    // Audit - track which agent created this builder
    public Guid? CreatedByAgentId { get; set; }
    
    // Navigation properties
    public Agent? CreatedByAgent { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
