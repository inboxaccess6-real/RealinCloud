using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("media", "realin");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(m => m.StorageBucket)
            .HasColumnName("storage_bucket")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100);

        builder.Property(m => m.Url)
            .HasColumnName("url")
            .HasMaxLength(1000);

        builder.Property(m => m.UploadedAt)
            .HasColumnName("uploaded_at")
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(m => m.PropertyId);

        // Relationships
        builder.HasOne(m => m.Property)
            .WithMany(p => p.MediaFiles)
            .HasForeignKey(m => m.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
