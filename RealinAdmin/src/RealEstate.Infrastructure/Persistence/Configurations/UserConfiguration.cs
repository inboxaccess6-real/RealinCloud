using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "realin");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(255);

        builder.Property(u => u.Provider)
            .HasColumnName("provider")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.OAuthProviderId)
            .HasColumnName("oauth_provider_id");

        builder.Property(u => u.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.IsBlocked)
            .HasColumnName("is_blocked")
            .HasDefaultValue(false);

        builder.Property(u => u.IsBlacklisted)
            .HasColumnName("is_blacklisted")
            .HasDefaultValue(false);

        builder.Property(u => u.BlacklistReason)
            .HasColumnName("blacklist_reason")
            .HasMaxLength(500);

        builder.Property(u => u.BlockedBy)
            .HasColumnName("blocked_by");

        builder.Property(u => u.BlockedAt)
            .HasColumnName("blocked_at");

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(u => u.DeletedBy)
            .HasColumnName("deleted_by");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.PhoneNumber).IsUnique();
        builder.HasIndex(u => u.RoleId);
        builder.HasIndex(u => new { u.Provider, u.OAuthProviderId }).IsUnique();

        // Check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_User_ContactMethod", "email IS NOT NULL OR phone_number IS NOT NULL"));

        // Relationships
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
