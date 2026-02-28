using Microsoft.EntityFrameworkCore;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpSession> OtpSessions => Set<OtpSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Builder> Builders => Set<Builder>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        NormalizeDateTimesToUtc();
        return base.SaveChanges();
    }

    private void NormalizeDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                {
                    prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("realin");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        SeedRoles(modelBuilder);
        SeedModules(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 11, 25, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                RoleType = RoleType.Guest,
                Name = "Guest",
                Description = "Guest user with limited access",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                RoleType = RoleType.User,
                Name = "User",
                Description = "Registered user with standard access",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                RoleType = RoleType.Agent,
                Name = "Agent",
                Description = "Real estate agent with property management access",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                RoleType = RoleType.Admin,
                Name = "Admin",
                Description = "Administrator with elevated access",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                RoleType = RoleType.SuperAdmin,
                Name = "SuperAdmin",
                Description = "Super administrator with full system access",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }

    private static void SeedModules(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 11, 25, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Module>().HasData(
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "USER_MGMT",
                Name = "User Management",
                Description = "Manage users, roles, and permissions",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "PROPERTY_BROWSE",
                Name = "Property Browse",
                Description = "Browse and search properties",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "PROPERTY_MGMT",
                Name = "Property Management",
                Description = "Create, update, and manage property listings",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "BOOKMARKS",
                Name = "Bookmarks",
                Description = "Save and manage favorite properties",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Code = "ANALYTICS",
                Name = "Analytics",
                Description = "View analytics and reports",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "SETTINGS",
                Name = "Settings",
                Description = "Manage system settings and configurations",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                Code = "BLACKLIST",
                Name = "Blacklist",
                Description = "Manage blacklisted users and entities",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
                Code = "AUDIT_LOGS",
                Name = "Audit Logs",
                Description = "View audit trail and activity logs",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000009"),
                Code = "REPORTS",
                Name = "Reports",
                Description = "Generate and view reports",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-00000000000a"),
                Code = "APPROVALS",
                Name = "Approvals",
                Description = "Manage approval workflows",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-00000000000b"),
                Code = "AGENT_MGMT",
                Name = "Agent Management",
                Description = "Manage agents, approvals, and agent profiles",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Module
            {
                Id = Guid.Parse("10000000-0000-0000-0000-00000000000c"),
                Code = "ROLE_MGMT",
                Name = "Role Management",
                Description = "Manage roles and permission assignments",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }
}
