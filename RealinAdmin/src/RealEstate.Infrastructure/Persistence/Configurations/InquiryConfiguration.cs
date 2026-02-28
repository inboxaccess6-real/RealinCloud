using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.ToTable("inquiries", "realin");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(i => i.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(i => i.Message)
            .HasColumnName("message")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(i => i.ContactPhone)
            .HasColumnName("contact_phone")
            .HasMaxLength(20);

        builder.Property(i => i.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(255);

        builder.Property(i => i.PreferredContactTime)
            .HasColumnName("preferred_contact_time")
            .HasMaxLength(100);

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasDefaultValue(InquiryStatus.Pending)
            .HasConversion<string>();

        builder.Property(i => i.Response)
            .HasColumnName("response")
            .HasMaxLength(2000);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(i => i.RespondedAt)
            .HasColumnName("responded_at");

        // Indexes
        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.PropertyId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.CreatedAt);

        // Relationships
        builder.HasOne(i => i.User)
            .WithMany(u => u.Inquiries)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Property)
            .WithMany(p => p.Inquiries)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
