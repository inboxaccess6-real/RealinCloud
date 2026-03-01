using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.ExternalAuth;
using RealEstate.Infrastructure.Identity;
using RealEstate.Infrastructure.Messaging;
using RealEstate.Infrastructure.Persistence;
using RealEstate.Infrastructure.Persistence.Repositories;
using RealEstate.Infrastructure.Storage;

namespace RealEstate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database — use DATABASE_URL if available, otherwise fall back to appsettings
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        string connectionString;

        if (!string.IsNullOrEmpty(databaseUrl))
        {
            // Support both postgres:// and postgresql:// schemes
            var normalized = databaseUrl.StartsWith("postgres://")
                ? "postgresql://" + databaseUrl["postgres://".Length..]
                : databaseUrl;
            var uri = new Uri(normalized);
            var userInfo = uri.UserInfo.Split(':');

            // Read sslmode from query string (Neon: require, Fly.io: disable)
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var sslMode = query["sslmode"] switch
            {
                "require" => "Require",
                "disable" => "Disable",
                "prefer" => "Prefer",
                _ => "Require" // default to SSL for safety
            };

            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode={sslMode};SearchPath=realin";
        }
        else
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No database connection configured. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IBuilderRepository, BuilderRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        var storageProvider = configuration["Storage:Provider"]?.ToLowerInvariant();
        if (storageProvider == "s3")
            services.AddScoped<IFileStorageService, S3StorageService>();
        else
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IAppleAuthService, AppleAuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        // Infrastructure
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddMemoryCache();
        services.AddHttpClient();

        return services;
    }
}
