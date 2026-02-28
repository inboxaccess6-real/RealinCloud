namespace RealEstate.Domain.Entities;

public class Inquiry
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid PropertyId { get; set; }

    public required string Message { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? PreferredContactTime { get; set; }

    public InquiryStatus Status { get; set; } = InquiryStatus.Pending;
    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
}
