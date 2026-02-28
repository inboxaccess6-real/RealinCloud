namespace RealEstate.Domain.Entities;

public class Favorite
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid PropertyId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
}
