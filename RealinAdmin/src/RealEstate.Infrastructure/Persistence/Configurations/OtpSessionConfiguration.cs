using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class OtpSessionConfiguration : IEntityTypeConfiguration<OtpSession>
{
    public void Configure(EntityTypeBuilder<OtpSession> builder)
    {
        builder.ToTable("otp_sessions", "realin");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(o => o.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(o => o.OtpCode)
            .HasColumnName("otp_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(o => o.DeliveryMethod)
            .HasColumnName("delivery_method")
            .HasConversion<string>();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(o => o.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(o => o.IsVerified)
            .HasColumnName("is_verified");

        builder.Property(o => o.AttemptCount)
            .HasColumnName("attempt_count");

        builder.Property(o => o.UserId)
            .HasColumnName("user_id");

        // Indexes
        builder.HasIndex(o => o.Email);
        builder.HasIndex(o => o.PhoneNumber);
        builder.HasIndex(o => o.CreatedAt);

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany(u => u.OtpSessions)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
