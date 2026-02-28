using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "realin");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("id");

        builder.Property(rp => rp.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rp => rp.ModuleId)
            .HasColumnName("module_id")
            .IsRequired();

        builder.Property(rp => rp.CanRead)
            .HasColumnName("can_read")
            .HasDefaultValue(false);

        builder.Property(rp => rp.CanCreate)
            .HasColumnName("can_create")
            .HasDefaultValue(false);

        builder.Property(rp => rp.CanUpdate)
            .HasColumnName("can_update")
            .HasDefaultValue(false);

        builder.Property(rp => rp.CanDelete)
            .HasColumnName("can_delete")
            .HasDefaultValue(false);

        builder.Property(rp => rp.CanManage)
            .HasColumnName("can_manage")
            .HasDefaultValue(false);

        builder.Property(rp => rp.CanExport)
            .HasColumnName("can_export")
            .HasDefaultValue(false);

        builder.Property(rp => rp.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(rp => rp.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(rp => rp.RoleId);
        builder.HasIndex(rp => rp.ModuleId);
        builder.HasIndex(rp => new { rp.RoleId, rp.ModuleId }).IsUnique();

        // Relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Module)
            .WithMany(m => m.RolePermissions)
            .HasForeignKey(rp => rp.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
