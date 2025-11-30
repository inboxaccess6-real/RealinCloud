namespace RealinApi.Data.Entities;

/// <summary>
/// Media/photos for properties
/// </summary>
public class Media
{
    public Guid Id { get; set; }
    public required Guid PropertyId { get; set; }
    public required string StorageBucket { get; set; }
    public required string StorageKey { get; set; }
    public string? Url { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation property
    public Property Property { get; set; } = null!;
}
