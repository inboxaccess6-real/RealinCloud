namespace RealinApi.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Name { get; set; }
    public required AuthProvider Provider { get; set; }
    
    // OAuth fields
    public string? OAuthProviderId { get; set; }  // Google/Apple user ID
    
    // Role relationship
    public required Guid RoleId { get; set; }
    
    // Account status
    public bool IsActive { get; set; } = true;
    
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
