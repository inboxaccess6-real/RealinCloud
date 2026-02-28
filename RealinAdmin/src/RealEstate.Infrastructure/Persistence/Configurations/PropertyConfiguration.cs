using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties", "realin");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(p => p.ListingType)
            .HasColumnName("listing_type")
            .IsRequired();

        builder.Property(p => p.ListingCategory)
            .HasColumnName("listing_category")
            .IsRequired();

        builder.Property(p => p.PropertyType)
            .HasColumnName("property_type")
            .IsRequired();

        builder.Property(p => p.ConstructionStatus)
            .HasColumnName("construction_status")
            .IsRequired();

        builder.Property(p => p.ReraId)
            .HasColumnName("rera_id");

        builder.Property(p => p.AvailableFrom)
            .HasColumnName("available_from");

        builder.Property(p => p.Status)
            .HasColumnName("status");

        builder.Property(p => p.AgentId)
            .HasColumnName("agent_id")
            .IsRequired();

        builder.Property(p => p.ProjectId)
            .HasColumnName("project_id");

        // Address
        builder.Property(p => p.Address)
            .HasColumnName("address");

        builder.Property(p => p.Locality)
            .HasColumnName("locality");

        builder.Property(p => p.City)
            .HasColumnName("city");

        builder.Property(p => p.PinCode)
            .HasColumnName("pin_code");

        builder.Property(p => p.Landmark)
            .HasColumnName("landmark");

        builder.Property(p => p.Latitude)
            .HasColumnName("latitude");

        builder.Property(p => p.Longitude)
            .HasColumnName("longitude");

        // Floor info
        builder.Property(p => p.FloorNumber)
            .HasColumnName("floor_number");

        builder.Property(p => p.TotalFloors)
            .HasColumnName("total_floors");

        builder.Property(p => p.Facing)
            .HasColumnName("facing")
            .IsRequired();

        // Pricing
        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency");

        builder.Property(p => p.MonthlyRent)
            .HasColumnName("monthly_rent")
            .HasPrecision(18, 2);

        builder.Property(p => p.SecurityDeposit)
            .HasColumnName("security_deposit")
            .HasPrecision(18, 2);

        builder.Property(p => p.MaintenanceCharges)
            .HasColumnName("maintenance_charges")
            .HasPrecision(18, 2);

        builder.Property(p => p.PriceNegotiable)
            .HasColumnName("price_negotiable");

        // Area & layout
        builder.Property(p => p.CarpetArea)
            .HasColumnName("carpet_area")
            .HasPrecision(10, 2);

        builder.Property(p => p.BuiltupArea)
            .HasColumnName("builtup_area")
            .HasPrecision(10, 2);

        builder.Property(p => p.SuperBuiltupArea)
            .HasColumnName("super_builtup_area")
            .HasPrecision(10, 2);

        builder.Property(p => p.Bedrooms)
            .HasColumnName("bedrooms");

        builder.Property(p => p.Bathrooms)
            .HasColumnName("bathrooms");

        builder.Property(p => p.Balconies)
            .HasColumnName("balconies");

        builder.Property(p => p.FurnishingStatus)
            .HasColumnName("furnishing_status");

        builder.Property(p => p.PropertyAge)
            .HasColumnName("property_age");

        // Legal and seller info
        builder.Property(p => p.OwnershipType)
            .HasColumnName("ownership_type");

        builder.Property(p => p.LoanAvailable)
            .HasColumnName("loan_available");

        // Media
        builder.Property(p => p.VideoUrl)
            .HasColumnName("video_url");

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url");

        // JSONB features
        builder.Property(p => p.Amenities)
            .HasColumnName("amenities")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'");

        builder.Property(p => p.InteriorFeatures)
            .HasColumnName("interior_features")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'");

        builder.Property(p => p.Utilities)
            .HasColumnName("utilities")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'");

        // Flags
        builder.Property(p => p.IsPublished)
            .HasColumnName("is_published");

        builder.Property(p => p.IsFeatured)
            .HasColumnName("is_featured");

        // Approval workflow
        builder.Property(p => p.ApprovalStatus)
            .HasColumnName("approval_status");

        builder.Property(p => p.RejectionReason)
            .HasColumnName("rejection_reason");

        builder.Property(p => p.ReviewedBy)
            .HasColumnName("reviewed_by");

        builder.Property(p => p.ReviewedAt)
            .HasColumnName("reviewed_at");

        builder.Property(p => p.SubmittedAt)
            .HasColumnName("submitted_at");

        // Content moderation
        builder.Property(p => p.IsFlagged)
            .HasColumnName("is_flagged");

        builder.Property(p => p.FlagReason)
            .HasColumnName("flag_reason");

        // Soft delete
        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(p => p.DeletedBy)
            .HasColumnName("deleted_by");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.AgentId);
        builder.HasIndex(p => p.City);
        builder.HasIndex(p => p.ListingCategory);
        builder.HasIndex(p => p.PropertyType);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsPublished);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.ApprovalStatus);

        // Relationships
        builder.HasOne(p => p.Agent)
            .WithMany(a => a.Properties)
            .HasForeignKey(p => p.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Project)
            .WithMany(pr => pr.Properties)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
