namespace RealEstate.Domain.Entities;

public class Agent
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? AgencyName { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? Rating { get; set; }

    // Verification workflow
    public string Status { get; set; } = "pending";
    public string? VerificationNotes { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? IdProofUrl { get; set; }
    public string? CompanyDetails { get; set; }

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

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<Builder> CreatedBuilders { get; set; } = new List<Builder>();
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
}
