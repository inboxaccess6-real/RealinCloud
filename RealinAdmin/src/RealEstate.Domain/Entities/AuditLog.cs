namespace RealEstate.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public required Guid EntityId { get; set; }
    public required Guid PerformedBy { get; set; }
    public string Details { get; set; } = "{}";
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public User PerformedByUser { get; set; } = null!;
}
