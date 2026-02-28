using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("agents", "realin");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.LicenseNumber)
            .HasColumnName("license_number")
            .HasMaxLength(100);

        builder.Property(a => a.AgencyName)
            .HasColumnName("agency_name")
            .HasMaxLength(200);

        builder.Property(a => a.ExperienceYears)
            .HasColumnName("experience_years");

        builder.Property(a => a.Rating)
            .HasColumnName("rating")
            .HasPrecision(2, 1);

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasDefaultValue("pending");

        builder.Property(a => a.VerificationNotes)
            .HasColumnName("verification_notes")
            .HasMaxLength(1000);

        builder.Property(a => a.VerifiedBy)
            .HasColumnName("verified_by");

        builder.Property(a => a.VerifiedAt)
            .HasColumnName("verified_at");

        builder.Property(a => a.IdProofUrl)
            .HasColumnName("id_proof_url")
            .HasMaxLength(1000);

        builder.Property(a => a.CompanyDetails)
            .HasColumnName("company_details")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'");

        builder.Property(a => a.IsBlocked)
            .HasColumnName("is_blocked")
            .HasDefaultValue(false);

        builder.Property(a => a.IsBlacklisted)
            .HasColumnName("is_blacklisted")
            .HasDefaultValue(false);

        builder.Property(a => a.BlacklistReason)
            .HasColumnName("blacklist_reason")
            .HasMaxLength(500);

        builder.Property(a => a.BlockedBy)
            .HasColumnName("blocked_by");

        builder.Property(a => a.BlockedAt)
            .HasColumnName("blocked_at");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(a => a.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(a => a.UserId).IsUnique();
        builder.HasIndex(a => a.LicenseNumber).IsUnique();
        builder.HasIndex(a => a.Status);

        // Relationships (one-to-one with User)
        builder.HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<Agent>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
