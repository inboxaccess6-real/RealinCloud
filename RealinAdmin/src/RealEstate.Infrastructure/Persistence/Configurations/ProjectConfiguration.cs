using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects", "realin");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.ReraId)
            .HasColumnName("rera_id")
            .HasMaxLength(100);

        builder.Property(p => p.BuilderId)
            .HasColumnName("builder_id")
            .IsRequired();

        builder.Property(p => p.Address)
            .HasColumnName("address")
            .HasMaxLength(1000);

        builder.Property(p => p.Locality)
            .HasColumnName("locality")
            .HasMaxLength(200);

        builder.Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(p => p.PinCode)
            .HasColumnName("pin_code")
            .HasMaxLength(10);

        builder.Property(p => p.Landmark)
            .HasColumnName("landmark")
            .HasMaxLength(200);

        builder.Property(p => p.Latitude)
            .HasColumnName("latitude");

        builder.Property(p => p.Longitude)
            .HasColumnName("longitude");

        builder.Property(p => p.ConstructionStatus)
            .HasColumnName("construction_status")
            .HasMaxLength(50);

        builder.Property(p => p.LaunchDate)
            .HasColumnName("launch_date");

        builder.Property(p => p.PossessionDate)
            .HasColumnName("possession_date");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(p => p.TotalTowers)
            .HasColumnName("total_towers");

        builder.Property(p => p.TotalUnits)
            .HasColumnName("total_units");

        builder.Property(p => p.IsBlocked)
            .HasColumnName("is_blocked")
            .HasDefaultValue(false);

        builder.Property(p => p.IsBlacklisted)
            .HasColumnName("is_blacklisted")
            .HasDefaultValue(false);

        builder.Property(p => p.BlacklistReason)
            .HasColumnName("blacklist_reason")
            .HasMaxLength(500);

        builder.Property(p => p.BlockedBy)
            .HasColumnName("blocked_by");

        builder.Property(p => p.BlockedAt)
            .HasColumnName("blocked_at");

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.CreatedByAgentId)
            .HasColumnName("created_by_agent_id");

        // Indexes
        builder.HasIndex(p => p.ReraId);
        builder.HasIndex(p => p.BuilderId);
        builder.HasIndex(p => p.City);

        // Relationships
        builder.HasOne(p => p.Builder)
            .WithMany(b => b.Projects)
            .HasForeignKey(p => p.BuilderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedByAgent)
            .WithMany(a => a.CreatedProjects)
            .HasForeignKey(p => p.CreatedByAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
