using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class BuilderConfiguration : IEntityTypeConfiguration<Builder>
{
    public void Configure(EntityTypeBuilder<Builder> builder)
    {
        builder.ToTable("builders", "realin");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(b => b.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(b => b.EstablishedYear)
            .HasColumnName("established_year");

        builder.Property(b => b.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(100);

        builder.Property(b => b.HeadquartersAddress)
            .HasColumnName("headquarters_address")
            .HasMaxLength(1000);

        builder.Property(b => b.Website)
            .HasColumnName("website")
            .HasMaxLength(500);

        builder.Property(b => b.Active)
            .HasColumnName("active")
            .HasDefaultValue(true);

        builder.Property(b => b.IsBlocked)
            .HasColumnName("is_blocked")
            .HasDefaultValue(false);

        builder.Property(b => b.IsBlacklisted)
            .HasColumnName("is_blacklisted")
            .HasDefaultValue(false);

        builder.Property(b => b.BlacklistReason)
            .HasColumnName("blacklist_reason")
            .HasMaxLength(500);

        builder.Property(b => b.BlockedBy)
            .HasColumnName("blocked_by");

        builder.Property(b => b.BlockedAt)
            .HasColumnName("blocked_at");

        builder.Property(b => b.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.CreatedByAgentId)
            .HasColumnName("created_by_agent_id");

        // Indexes
        builder.HasIndex(b => b.Name);
        builder.HasIndex(b => b.Email);

        // Relationships
        builder.HasOne(b => b.CreatedByAgent)
            .WithMany(a => a.CreatedBuilders)
            .HasForeignKey(b => b.CreatedByAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
