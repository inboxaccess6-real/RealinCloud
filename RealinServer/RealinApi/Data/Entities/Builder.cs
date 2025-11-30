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
    public DateTime CreatedAt { get; set; }
    
    // Audit - track which agent created this builder
    public Guid? CreatedByAgentId { get; set; }
    
    // Navigation properties
    public Agent? CreatedByAgent { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
