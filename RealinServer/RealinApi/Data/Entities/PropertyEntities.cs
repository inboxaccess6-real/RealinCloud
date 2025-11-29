namespace RealinApi.Data.Entities;

/// <summary>
/// Represents a real estate builder/developer
/// </summary>
public class Builder
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? EstablishedYear { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? HeadquartersAddress { get; set; }
    public string? Website { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Audit - track which agent created this builder
    public Guid? CreatedByAgentId { get; set; }
    
    // Navigation properties
    public Agent? CreatedByAgent { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

/// <summary>
/// Represents a real estate project by a builder
/// </summary>
public class Project
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? ReraId { get; set; }
    public required Guid BuilderId { get; set; }
    
    // Embedded address
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? City { get; set; }
    public string? PinCode { get; set; }
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Construction details
    public string? ConstructionStatus { get; set; } // ready / under_construction / new_launch
    public DateTime? LaunchDate { get; set; }
    public DateTime? PossessionDate { get; set; }
    public string? Status { get; set; } // active / completed / on_hold
    public int? TotalTowers { get; set; }
    public int? TotalUnits { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Audit - track which agent created this project
    public Guid? CreatedByAgentId { get; set; }
    
    // Navigation properties
    public Builder Builder { get; set; } = null!;
    public Agent? CreatedByAgent { get; set; }
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}

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
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Agent Agent { get; set; } = null!; // Every property has an agent
    public Project? Project { get; set; } // Property may belong to a project
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();
}

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
}
