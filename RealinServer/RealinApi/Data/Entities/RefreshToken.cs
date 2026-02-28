namespace RealinApi.Data.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public required Guid UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? DeviceInfo { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
