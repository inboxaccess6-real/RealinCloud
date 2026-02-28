namespace RealinApi.Data.Entities;

/// <summary>
/// Tracks all admin actions for audit trail
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public required string Action { get; set; } // e.g. "user.blocked", "property.approved", "agent.rejected"
    public required string EntityType { get; set; } // e.g. "User", "Property", "Agent", "Builder", "Project"
    public required Guid EntityId { get; set; }
    public required Guid PerformedBy { get; set; }
    public string Details { get; set; } = "{}"; // JSONB - arbitrary context (old/new values, reason, etc.)
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public User PerformedByUser { get; set; } = null!;
}
