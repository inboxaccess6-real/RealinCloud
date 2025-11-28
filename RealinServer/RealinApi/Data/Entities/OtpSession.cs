namespace RealinApi.Data.Entities;

public enum OtpDeliveryMethod
{
    Sms,
    Email
}

public class OtpSession
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public required string OtpCode { get; set; }
    public required OtpDeliveryMethod DeliveryMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsVerified { get; set; }
    public int AttemptCount { get; set; }
    public Guid? UserId { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}
