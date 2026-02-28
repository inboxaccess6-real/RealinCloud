using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads", "realin");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(l => l.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(l => l.AssignedAgentId)
            .HasColumnName("assigned_agent_id");

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasDefaultValue(LeadStatus.New)
            .HasConversion<string>();

        builder.Property(l => l.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(l => l.Source)
            .HasColumnName("source")
            .HasMaxLength(50);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(l => l.ContactedAt)
            .HasColumnName("contacted_at");

        builder.Property(l => l.ConvertedAt)
            .HasColumnName("converted_at");

        // Indexes
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.PropertyId);
        builder.HasIndex(l => l.AssignedAgentId);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.CreatedAt);

        // Relationships
        builder.HasOne(l => l.User)
            .WithMany(u => u.Leads)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Property)
            .WithMany(p => p.Leads)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.AssignedAgent)
            .WithMany(a => a.AssignedLeads)
            .HasForeignKey(l => l.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
