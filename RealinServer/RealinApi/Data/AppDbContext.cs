using Microsoft.EntityFrameworkCore;
using RealinApi.Data.Entities;

namespace RealinApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpSession> OtpSessions => Set<OtpSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    
    // Property-related entities
    public DbSet<Builder> Builders => Set<Builder>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Agent> Agents => Set<Agent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema to 'realin' instead of 'public'
        modelBuilder.HasDefaultSchema("realin");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => new { e.Provider, e.OAuthProviderId }).IsUnique();
            entity.HasIndex(e => e.RoleId);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(e => e.Provider).HasColumnName("provider").IsRequired();
            entity.Property(e => e.OAuthProviderId).HasColumnName("oauth_provider_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting roles with users
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked");
            entity.Property(e => e.DeviceInfo).HasColumnName("device_info");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OtpSession configuration
        modelBuilder.Entity<OtpSession>(entity =>
        {
            entity.ToTable("otp_sessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(e => e.OtpCode).HasColumnName("otp_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.DeliveryMethod).HasColumnName("delivery_method");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.AttemptCount).HasColumnName("attempt_count");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.OtpSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RoleType).IsUnique();
            entity.HasIndex(e => e.Name);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleType).HasColumnName("role_type").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        // Module configuration
        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("modules");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        // RolePermission configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.ModuleId }).IsUnique(); // One permission set per role-module combination
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.ModuleId);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
            entity.Property(e => e.ModuleId).HasColumnName("module_id").IsRequired();
            entity.Property(e => e.CanRead).HasColumnName("can_read").HasDefaultValue(false);
            entity.Property(e => e.CanCreate).HasColumnName("can_create").HasDefaultValue(false);
            entity.Property(e => e.CanUpdate).HasColumnName("can_update").HasDefaultValue(false);
            entity.Property(e => e.CanDelete).HasColumnName("can_delete").HasDefaultValue(false);
            entity.Property(e => e.CanManage).HasColumnName("can_manage").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // Delete permissions when role is deleted
            
            entity.HasOne(e => e.Module)
                .WithMany(m => m.RolePermissions)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade); // Delete permissions when module is deleted
        });

        // ========================================
        // PROPERTY DOMAIN CONFIGURATIONS
        // ========================================
        
        // Builder configuration
        modelBuilder.Entity<Builder>(entity =>
        {
            entity.ToTable("builders");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Email);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.EstablishedYear).HasColumnName("established_year");
            entity.Property(e => e.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(100);
            entity.Property(e => e.HeadquartersAddress).HasColumnName("headquarters_address").HasMaxLength(1000);
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(500);
            entity.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedByAgentId).HasColumnName("created_by_agent_id");
            
            entity.HasOne(e => e.CreatedByAgent)
                .WithMany(a => a.CreatedBuilders)
                .HasForeignKey(e => e.CreatedByAgentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BuilderId);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.ReraId);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ReraId).HasColumnName("rera_id").HasMaxLength(100);
            entity.Property(e => e.BuilderId).HasColumnName("builder_id").IsRequired();
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(1000);
            entity.Property(e => e.Locality).HasColumnName("locality").HasMaxLength(200);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.PinCode).HasColumnName("pin_code").HasMaxLength(10);
            entity.Property(e => e.Landmark).HasColumnName("landmark").HasMaxLength(200);
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.ConstructionStatus).HasColumnName("construction_status").HasMaxLength(50);
            entity.Property(e => e.LaunchDate).HasColumnName("launch_date");
            entity.Property(e => e.PossessionDate).HasColumnName("possession_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.TotalTowers).HasColumnName("total_towers");
            entity.Property(e => e.TotalUnits).HasColumnName("total_units");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedByAgentId).HasColumnName("created_by_agent_id");
            
            entity.HasOne(e => e.Builder)
                .WithMany(b => b.Projects)
                .HasForeignKey(e => e.BuilderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CreatedByAgent)
                .WithMany(a => a.CreatedProjects)
                .HasForeignKey(e => e.CreatedByAgentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Property configuration
        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.ListingCategory);
            entity.HasIndex(e => e.PropertyType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.IsFeatured);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ListingType).HasColumnName("listing_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ListingCategory).HasColumnName("listing_category").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PropertyType).HasColumnName("property_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ConstructionStatus).HasColumnName("construction_status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ReraId).HasColumnName("rera_id").HasMaxLength(100);
            entity.Property(e => e.AvailableFrom).HasColumnName("available_from");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("active");
            entity.Property(e => e.AgentId).HasColumnName("agent_id").IsRequired();
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(1000);
            entity.Property(e => e.Locality).HasColumnName("locality").HasMaxLength(200);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.PinCode).HasColumnName("pin_code").HasMaxLength(10);
            entity.Property(e => e.Landmark).HasColumnName("landmark").HasMaxLength(200);
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.FloorNumber).HasColumnName("floor_number");
            entity.Property(e => e.TotalFloors).HasColumnName("total_floors");
            entity.Property(e => e.Facing).HasColumnName("facing").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(e => e.MonthlyRent).HasColumnName("monthly_rent").HasPrecision(18, 2);
            entity.Property(e => e.SecurityDeposit).HasColumnName("security_deposit").HasPrecision(18, 2);
            entity.Property(e => e.MaintenanceCharges).HasColumnName("maintenance_charges").HasPrecision(18, 2);
            entity.Property(e => e.PriceNegotiable).HasColumnName("price_negotiable");
            entity.Property(e => e.CarpetArea).HasColumnName("carpet_area").HasPrecision(10, 2);
            entity.Property(e => e.BuiltupArea).HasColumnName("builtup_area").HasPrecision(10, 2);
            entity.Property(e => e.SuperBuiltupArea).HasColumnName("super_builtup_area").HasPrecision(10, 2);
            entity.Property(e => e.Bedrooms).HasColumnName("bedrooms");
            entity.Property(e => e.Bathrooms).HasColumnName("bathrooms");
            entity.Property(e => e.Balconies).HasColumnName("balconies");
            entity.Property(e => e.FurnishingStatus).HasColumnName("furnishing_status").HasMaxLength(50);
            entity.Property(e => e.PropertyAge).HasColumnName("property_age");
            entity.Property(e => e.OwnershipType).HasColumnName("ownership_type").HasMaxLength(50);
            entity.Property(e => e.LoanAvailable).HasColumnName("loan_available");
            entity.Property(e => e.VideoUrl).HasColumnName("video_url").HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(1000);
            entity.Property(e => e.Amenities).HasColumnName("amenities").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.InteriorFeatures).HasColumnName("interior_features").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.Utilities).HasColumnName("utilities").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Agent)
                .WithMany(a => a.Properties)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Restrict); // Required relationship - prevent agent deletion if they have properties
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Properties)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull); // Optional relationship
        });
        
        // Media configuration
        modelBuilder.Entity<Media>(entity =>
        {
            entity.ToTable("media");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PropertyId);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PropertyId).HasColumnName("property_id").IsRequired();
            entity.Property(e => e.StorageBucket).HasColumnName("storage_bucket").HasMaxLength(200).IsRequired();
            entity.Property(e => e.StorageKey).HasColumnName("storage_key").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Url).HasColumnName("url").HasMaxLength(1000);
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.Property)
                .WithMany(p => p.MediaFiles)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Agent configuration
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("agents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(100);
            entity.Property(e => e.AgencyName).HasColumnName("agency_name").HasMaxLength(200);
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.Rating).HasColumnName("rating").HasPrecision(2, 1);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Agent>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default roles
        SeedRoles(modelBuilder);
        SeedModules(modelBuilder);
    }

    private void SeedRoles(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 11, 25, 0, 0, 0, DateTimeKind.Utc);
        
        var roles = new[]
        {
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                RoleType = RoleType.Guest,
                Name = "Guest",
                Description = "Limited access for unverified users",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                RoleType = RoleType.User,
                Name = "User",
                Description = "Standard verified user with basic access",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                RoleType = RoleType.Agent,
                Name = "Real Estate Agent",
                Description = "Real estate professional with property management capabilities",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                RoleType = RoleType.Admin,
                Name = "Administrator",
                Description = "System administrator with elevated privileges",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                RoleType = RoleType.SuperAdmin,
                Name = "Super Administrator",
                Description = "Full system access with all privileges",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        };

        modelBuilder.Entity<Role>().HasData(roles);
    }

    private void SeedModules(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 11, 25, 0, 0, 0, DateTimeKind.Utc);
        
        var modules = new[]
        {
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "USER_MGMT",
                Name = "User Management",
                Description = "Manage user accounts, roles, and permissions",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "PROPERTY_BROWSE",
                Name = "Property Browsing",
                Description = "View and search property listings (public access)",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "PROPERTY_MANAGE",
                Name = "Property Management",
                Description = "Create, edit, and manage property listings (agents only)",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "BOOKMARKS",
                Name = "Bookmarks & Favorites",
                Description = "Save and manage favorite properties",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Code = "ANALYTICS",
                Name = "Analytics & Reporting",
                Description = "View analytics, reports, and business insights",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "SETTINGS",
                Name = "System Settings",
                Description = "Configure system-wide settings and preferences",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        };

        modelBuilder.Entity<Module>().HasData(modules);
    }
}
