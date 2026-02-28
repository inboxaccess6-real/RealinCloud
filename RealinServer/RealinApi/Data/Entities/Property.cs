namespace RealinApi.Data.Entities;

/// <summary>
/// Core property listing entity
/// </summary>
public class Property
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string ListingType { get; set; } // owner / builder / agent / bank
    public required string ListingCategory { get; set; } // sale / rent / pg / flatmates
    public required string PropertyType { get; set; } // apartment / villa / plot / etc
    public required string ConstructionStatus { get; set; } // ready / under_construction / new_launch
    public string? ReraId { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public string Status { get; set; } = "active"; // active / sold / rented / off_market
    
    // Linking - every property MUST have an agent (who created/manages it)
    // Property may optionally belong to a project
    public required Guid AgentId { get; set; }
    public Guid? ProjectId { get; set; }
    
    // Embedded address
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? City { get; set; }
    public string? PinCode { get; set; }
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Floor info
    public int? FloorNumber { get; set; }
    public int? TotalFloors { get; set; }
    public required string Facing { get; set; } // north / south / east / west / northeast / etc
    
    // Pricing
    public required decimal Price { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public decimal? MaintenanceCharges { get; set; }
    public bool? PriceNegotiable { get; set; }
    
    // Area & layout
    public decimal? CarpetArea { get; set; }
    public decimal? BuiltupArea { get; set; }
    public decimal? SuperBuiltupArea { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Balconies { get; set; }
    public string? FurnishingStatus { get; set; } // unfurnished / semi / fully
    public int? PropertyAge { get; set; }
    
    // Legal and seller info
    public string? OwnershipType { get; set; } // freehold / leasehold
    public bool? LoanAvailable { get; set; }
    
    // Media
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    
    // JSONB features (stored as JSON strings in PostgreSQL)
    public string Amenities { get; set; } = "{}"; // JSON string
    public string InteriorFeatures { get; set; } = "{}"; // JSON string
    public string Utilities { get; set; } = "{}"; // JSON string
    
    // Flags
    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    // Approval workflow
    public string ApprovalStatus { get; set; } = "draft"; // draft / submitted / under_review / approved / rejected
    public string? RejectionReason { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Content moderation
    public bool IsFlagged { get; set; } = false;
    public string? FlagReason { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Agent Agent { get; set; } = null!; // Every property has an agent
    public Project? Project { get; set; } // Property may belong to a project
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
