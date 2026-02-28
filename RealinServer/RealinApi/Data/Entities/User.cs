namespace RealinApi.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Name { get; set; }
    public required AuthProvider Provider { get; set; }
    
    // OAuth fields
    public string? OAuthProviderId { get; set; }  // Google/Apple user ID
    
    // Role relationship
    public required Guid RoleId { get; set; }
    
    // Account status
    public bool IsActive { get; set; } = true;

    // Moderation
    public bool IsBlocked { get; set; } = false;
    public bool IsBlacklisted { get; set; } = false;
    public string? BlacklistReason { get; set; }
    public Guid? BlockedBy { get; set; }
    public DateTime? BlockedAt { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<OtpSession> OtpSessions { get; set; } = new List<OtpSession>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
