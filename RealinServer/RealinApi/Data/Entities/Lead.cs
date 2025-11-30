namespace RealinApi.Data.Entities;

/// <summary>
/// Lead status for tracking sales pipeline
/// </summary>
public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Negotiation,
    Converted,
    Lost
}

/// <summary>
/// Sales leads from property inquiries
/// </summary>
public class Lead
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid PropertyId { get; set; }
    public Guid? AssignedAgentId { get; set; }
    
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string? Notes { get; set; }
    public string? Source { get; set; } // web / app / referral
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ContactedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
    public Agent? AssignedAgent { get; set; }
}
