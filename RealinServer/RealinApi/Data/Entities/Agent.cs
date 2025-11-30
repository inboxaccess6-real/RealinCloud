namespace RealinApi.Data.Entities;

/// <summary>
/// Real estate agent/broker
/// </summary>
public class Agent
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? AgencyName { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<Builder> CreatedBuilders { get; set; } = new List<Builder>();
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
}
