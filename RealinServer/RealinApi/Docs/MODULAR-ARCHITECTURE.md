# RealinApi Modular Monolith Architecture

## Overview

This document describes the proposed modular monolith architecture for RealinApi. The architecture provides clear domain boundaries while maintaining the simplicity and deployment benefits of a monolithic application.

---

## Why Modular Monolith?

| Benefit | Description |
|---------|-------------|
| **Simplicity** | Single deployable unit without the complexity of microservices |
| **Clear Boundaries** | Domain modules with explicit contracts prevent tight coupling |
| **Easy Migration** | Can evolve to microservices by extracting modules if needed |
| **Shared Infrastructure** | Common database, authentication, and caching layer |
| **Development Speed** | Teams can work on modules independently with clear interfaces |

---

## Proposed Folder Structure

> **Design Decision**: Entities remain in `Data/Entities/` (shared) rather than being distributed across modules. This is the recommended approach for a modular monolith with a shared database because:
> - Single source of truth for database entities
> - EF Core migrations work seamlessly with all entities in one location
> - Cross-entity relationships (e.g., `Property.Agent`, `Favorite.User`) work naturally
> - Modules focus on business logic (services, endpoints, DTOs)

```
RealinApi/
├── Program.cs                         # Application entry point with module registration
├── appsettings.json
│
├── Data/                              # Database Layer (ALL entities here)
│   ├── Entities/                      # ← ALL entities stay in Data layer
│   │   ├── User.cs                    # Identity entities
│   │   ├── Role.cs
│   │   ├── Module.cs
│   │   ├── RolePermission.cs
│   │   ├── RefreshToken.cs
│   │   ├── OtpSession.cs
│   │   ├── Property.cs                # Catalog entities
│   │   ├── Project.cs
│   │   ├── Builder.cs
│   │   ├── Agent.cs
│   │   ├── Media.cs
│   │   ├── Favorite.cs                # Engagement entities
│   │   ├── Lead.cs
│   │   └── Inquiry.cs
│   ├── AppDbContext.cs                # Shared EF Core context
│   ├── SeedPermissions.cs
│   └── Migrations/
│
├── Modules/
│   ├── Identity/                      # Authentication & Authorization Module
│   │   ├── IdentityModule.cs          # Module registration (DI + Endpoints)
│   │   ├── Models/                    # DTOs only (no entities)
│   │   │   ├── AuthModels.cs          # Login/Register request/response DTOs
│   │   │   ├── UserModels.cs          # User CRUD DTOs
│   │   │   └── RoleModels.cs          # Role management DTOs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── JwtService.cs
│   │   │   ├── OtpService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── RoleService.cs
│   │   │   ├── ModuleService.cs
│   │   │   ├── PermissionService.cs
│   │   │   └── CurrentUserAccessor.cs
│   │   ├── ExternalProviders/
│   │   │   ├── GoogleAuthService.cs
│   │   │   └── AppleAuthService.cs
│   │   └── Endpoints/
│   │       ├── AuthEndpoints.cs
│   │       ├── UserEndpoints.cs
│   │       ├── RoleEndpoints.cs
│   │       └── ModuleEndpoints.cs
│   │
│   ├── Catalog/                       # Property Listings Module
│   │   ├── CatalogModule.cs
│   │   ├── Models/                    # DTOs only (no entities)
│   │   │   ├── PropertyModels.cs
│   │   │   ├── ProjectModels.cs
│   │   │   └── BuilderAgentModels.cs
│   │   ├── Services/
│   │   │   ├── PropertyService.cs
│   │   │   ├── ProjectService.cs
│   │   │   ├── BuilderService.cs
│   │   │   ├── AgentService.cs
│   │   │   └── MediaService.cs
│   │   └── Endpoints/
│   │       ├── PropertyEndpoints.cs
│   │       ├── ProjectEndpoints.cs
│   │       ├── BuilderEndpoints.cs
│   │       ├── AgentEndpoints.cs
│   │       └── MediaEndpoints.cs
│   │
│   ├── Engagement/                    # User Interactions Module
│   │   ├── EngagementModule.cs
│   │   ├── Models/                    # DTOs only (no entities)
│   │   │   └── EngagementModels.cs
│   │   ├── Services/
│   │   │   ├── FavoriteService.cs
│   │   │   └── LeadService.cs
│   │   └── Endpoints/
│   │       ├── FavoriteEndpoints.cs
│   │       └── LeadEndpoints.cs
│   │
│   └── Search/                        # Search & Discovery Module (Future)
│       ├── SearchModule.cs
│       ├── Models/
│       │   └── SearchModels.cs
│       ├── Services/
│       │   └── SearchService.cs
│       └── Endpoints/
│           └── SearchEndpoints.cs
│
├── Shared/                            # Shared Kernel (Cross-Module Contracts)
│   ├── Contracts/
│   │   ├── ICurrentUser.cs            # Current user context interface
│   │   ├── UserDto.cs                 # User reference DTO
│   │   └── PropertySummaryDto.cs      # Property reference DTO
│   ├── Exceptions/
│   │   └── DomainException.cs         # Base domain exceptions
│   └── Extensions/
│       ├── ValidationExtensions.cs
│       └── ResultExtensions.cs
│
├── Infrastructure/                    # Cross-Cutting Infrastructure
│   ├── Messaging/
│   │   ├── SmsService.cs
│   │   └── EmailService.cs
│   └── Storage/
│       └── FileStorageService.cs
│
└── Properties/
    └── launchSettings.json
```

### Key Architecture Decision: Entities in Data Layer

| Aspect | Entities in Data/ | Entities in Modules/ |
|--------|-------------------|---------------------|
| **Refactoring effort** | Low ✅ | High |
| **EF Core compatibility** | Easy ✅ | Complex |
| **Cross-entity relations** | Natural ✅ | Tricky |
| **Module encapsulation** | Partial | Full ✅ |
| **Recommended for** | Modular Monolith ✅ | Microservices |

**Conclusion**: For a modular monolith with a shared database, keeping entities in `Data/Entities/` provides 80% of the modular benefits with 20% of the refactoring effort.

---

## Domain Modules

### 1. Identity Module

**Purpose**: Manages authentication, authorization, users, and roles.

**Responsibilities**:
- User registration and profile management
- Multi-provider authentication (Email/Phone OTP, Google, Apple)
- JWT token generation and refresh
- Role-Based Access Control (RBAC)
- Module-level permissions

**Entities**:
| Entity | Description |
|--------|-------------|
| `User` | User accounts with profile information |
| `Role` | Role definitions (Guest, User, Agent, Admin, SuperAdmin) |
| `Module` | Permission modules for RBAC |
| `RolePermission` | Many-to-many role-module permissions |
| `RefreshToken` | JWT refresh tokens |
| `OtpSession` | OTP verification sessions |

**Endpoints**:
| Route | Description |
|-------|-------------|
| `POST /api/auth/otp/request` | Request OTP for email/phone |
| `POST /api/auth/otp/verify` | Verify OTP and get tokens |
| `POST /api/auth/google` | Google OAuth login |
| `POST /api/auth/apple` | Apple OAuth login |
| `POST /api/auth/refresh` | Refresh JWT token |
| `GET /api/users` | List users (Admin) |
| `GET /api/users/{id}` | Get user details |
| `PUT /api/users/{id}/role` | Update user role (SuperAdmin) |
| `GET /api/roles` | List roles |
| `POST /api/roles` | Create role (SuperAdmin) |
| `GET /api/modules` | List permission modules |

---

### 2. Catalog Module

**Purpose**: Manages property listings and real estate inventory.

**Responsibilities**:
- Property CRUD with filtering and search
- Project management (multi-property developments)
- Builder/Developer profiles
- Agent management
- Media handling (images, documents)

**Entities**:
| Entity | Description |
|--------|-------------|
| `Property` | Individual property listings |
| `Project` | Multi-property development projects |
| `Builder` | Real estate developers/builders |
| `Agent` | Real estate agents |
| `Media` | Images and documents for properties |

**Endpoints**:
| Route | Description |
|-------|-------------|
| `GET /api/properties` | List properties with filters |
| `GET /api/properties/{id}` | Get property details |
| `POST /api/properties` | Create property (Agent/Admin) |
| `PUT /api/properties/{id}` | Update property |
| `DELETE /api/properties/{id}` | Delete property |
| `GET /api/projects` | List projects |
| `GET /api/projects/{id}` | Get project with properties |
| `POST /api/projects` | Create project (Admin) |
| `GET /api/builders` | List builders |
| `GET /api/agents` | List agents |
| `POST /api/agents/apply` | Apply to become agent |

---

### 3. Engagement Module

**Purpose**: Manages user interactions with properties.

**Responsibilities**:
- Favorite properties management
- Lead capture and tracking
- Inquiry handling
- User engagement analytics

**Entities**:
| Entity | Description |
|--------|-------------|
| `Favorite` | User's saved/favorited properties |
| `Lead` | Sales leads from property inquiries |
| `Inquiry` | User questions and contact requests |

**Endpoints**:
| Route | Description |
|-------|-------------|
| `GET /api/favorites` | Get user's favorites |
| `POST /api/favorites` | Add property to favorites |
| `DELETE /api/favorites/{propertyId}` | Remove from favorites |
| `GET /api/favorites/check/{propertyId}` | Check if property is favorited |
| `GET /api/leads` | List leads (Agent/Admin) |
| `GET /api/leads/{id}` | Get lead details |
| `POST /api/leads` | Create lead from inquiry |
| `PUT /api/leads/{id}/status` | Update lead status |
| `PUT /api/leads/{id}/assign` | Assign lead to agent |

---

## Module Registration Pattern

Each module should expose two extension methods:

### Module File Template

```csharp
// Modules/Identity/IdentityModule.cs
namespace RealinApi.Modules.Identity;

public static class IdentityModule
{
    /// <summary>
    /// Registers all Identity module services with DI
    /// </summary>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Core authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IOtpService, OtpService>();
        
        // External auth providers
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IAppleAuthService, AppleAuthService>();
        
        // User & Role management
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IModuleService, ModuleService>();
        
        // Permission service
        services.AddScoped<IPermissionService, PermissionService>();
        
        // Current user accessor (bridge to other modules)
        services.AddScoped<ICurrentUser, CurrentUserAccessor>();
        
        return services;
    }
    
    /// <summary>
    /// Maps all Identity module endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapRoleEndpoints();
        app.MapModuleEndpoints();
        
        return app;
    }
}
```

### Program.cs Usage

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddAuthentication(...);

// ===== Module Registration =====
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule();
builder.Services.AddEngagementModule();

var app = builder.Build();

// ===== Endpoint Mapping =====
app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapEngagementEndpoints();

app.Run();
```

### Module Service Example (Referencing Entities from Data Layer)

```csharp
// Modules/Catalog/Services/PropertyService.cs
using Microsoft.EntityFrameworkCore;
using RealinApi.Data;                      // ← AppDbContext
using RealinApi.Data.Entities;             // ← Entities from Data layer
using RealinApi.Modules.Catalog.Models;    // ← Module-specific DTOs
using RealinApi.Shared.Contracts;          // ← Shared kernel

namespace RealinApi.Modules.Catalog.Services;

public interface IPropertyService
{
    Task<PropertyDetailResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyListItemResponse>> SearchAsync(PropertySearchRequest request);
    Task<PropertyDetailResponse> CreateAsync(CreatePropertyRequest request);
}

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;      // Shared DbContext
    private readonly ICurrentUser _currentUser;  // From Shared Kernel

    public PropertyService(AppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PropertyDetailResponse?> GetByIdAsync(Guid id)
    {
        // Query entity from Data layer
        var property = await _context.Properties
            .Include(p => p.Agent)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (property == null) return null;
        
        // Map Entity → DTO (Module's responsibility)
        return MapToDetailResponse(property);
    }

    public async Task<PropertyDetailResponse> CreateAsync(CreatePropertyRequest request)
    {
        // Create entity (defined in Data/Entities)
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            PropertyType = request.PropertyType,
            Price = request.Price,
            AgentId = _currentUser.UserId,  // From Shared Kernel
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        
        return MapToDetailResponse(property);
    }

    // Private mapping method: Entity → DTO
    private static PropertyDetailResponse MapToDetailResponse(Property entity)
    {
        return new PropertyDetailResponse(
            Id: entity.Id,
            Title: entity.Title,
            PropertyType: entity.PropertyType,
            Price: entity.Price,
            // ... other mappings
        );
    }
}
```

### Import Pattern Summary

| Import | Purpose |
|--------|---------|
| `using RealinApi.Data;` | Access to `AppDbContext` |
| `using RealinApi.Data.Entities;` | Access to entity classes |
| `using RealinApi.Modules.{Module}.Models;` | Module-specific DTOs |
| `using RealinApi.Shared.Contracts;` | Cross-module interfaces |
| `using RealinApi.Shared.Exceptions;` | Domain exceptions |

---

## Shared Kernel

The Shared Kernel contains cross-cutting concerns that all modules can depend on:

### ICurrentUser Interface

```csharp
// Shared/Contracts/ICurrentUser.cs
namespace RealinApi.Shared.Contracts;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string RoleName { get; }
    int RoleLevel { get; }
    bool IsAuthenticated { get; }
    
    Task<bool> HasPermissionAsync(string moduleCode, string permission);
    bool HasMinimumRole(int minimumRoleLevel);
}
```

### Cross-Module DTOs

```csharp
// Shared/Contracts/UserDto.cs
namespace RealinApi.Shared.Contracts;

public record UserDto(
    Guid Id, 
    string Email, 
    string? FullName, 
    string Role
);

// Shared/Contracts/PropertySummaryDto.cs
public record PropertySummaryDto(
    Guid Id, 
    string Title, 
    decimal Price, 
    string City
);
```

### Domain Exceptions

```csharp
// Shared/Exceptions/DomainException.cs
namespace RealinApi.Shared.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }
    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object id) 
        : base("NOT_FOUND", $"{entity} with id '{id}' was not found") { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized") 
        : base("UNAUTHORIZED", message) { }
}

public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }
    public ValidationException(Dictionary<string, string[]> errors) 
        : base("VALIDATION_ERROR", "One or more validation errors occurred")
    {
        Errors = errors;
    }
}
```

---

## Cross-Module Communication

Modules communicate through the **Shared Kernel** contracts, not direct dependencies:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Identity       │     │   Catalog       │     │  Engagement     │
│    Module       │     │    Module       │     │    Module       │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Shared Kernel                             │
│  (ICurrentUser, DTOs, Exceptions, Extensions)                   │
└─────────────────────────────────────────────────────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer                        │
│  (AppDbContext, Messaging, Storage)                             │
└─────────────────────────────────────────────────────────────────┘
```

### Example: Engagement Module Using Identity Context

```csharp
// Modules/Engagement/Services/FavoriteService.cs
public class FavoriteService(
    AppDbContext db, 
    ICurrentUser currentUser)  // From Shared Kernel
{
    public async Task<List<FavoriteResponse>> GetUserFavoritesAsync()
    {
        // Uses ICurrentUser from Shared Kernel
        return await db.Favorites
            .Where(f => f.UserId == currentUser.UserId)
            .Include(f => f.Property)
            .Select(f => new FavoriteResponse(...))
            .ToListAsync();
    }
}
```

---

## Database Schema

All entities share the PostgreSQL database with the `realin` schema:

```sql
-- Identity Module
realin.users
realin.roles
realin.modules
realin.role_permissions
realin.refresh_tokens
realin.otp_sessions

-- Catalog Module
realin.properties
realin.projects
realin.builders
realin.agents
realin.media

-- Engagement Module
realin.favorites
realin.leads
realin.inquiries
```

---

## Role Hierarchy

```
SuperAdmin (Level 5) - all permissions
    └── Admin (Level 4) - user management + all below
        └── Agent (Level 3) - property management + all below
            └── User (Level 2) - browse + favorites + leads
                └── Guest (Level 1) - browse only
```

---

## Module Dependencies Rules

### ✅ Allowed Dependencies

```
Module → Shared Kernel
Module → Infrastructure (AppDbContext, Messaging)
```

### ❌ Not Allowed

```
Module → Another Module (use Shared contracts instead)
Circular dependencies between modules
```

---

## Migration Steps

### Step 1: Create Shared Kernel (Optional)
1. Create `Shared/Contracts/` with `ICurrentUser`, cross-module interfaces
2. Create `Shared/Exceptions/` with domain exceptions
3. Create `Shared/Extensions/` with utilities

> **Note:** The Shared Kernel is optional. You can start with modules importing directly from `Common/` and `Infrastructure/`.

### Step 2: Create Identity Module
1. Create `Modules/Identity/` folder structure:
   - `Services/` - Move from `Features/Auth/Services/` and `Features/Admin/Services/`
   - `Endpoints/` - Move from `Features/Auth/Endpoints/` and `Features/Admin/Endpoints/`
   - `Models/` - Create DTOs specific to Identity (LoginRequest, AuthResponse, etc.)
2. Update services to import entities from `RealinApi.Data.Entities`
3. Create `IdentityModule.cs` with registration methods
4. **Keep entities in `Data/Entities/`** - do NOT move them

### Step 3: Create Catalog Module
1. Create `Modules/Catalog/` folder structure:
   - `Services/` - Move from `Features/Property/Services/`
   - `Endpoints/` - Move from `Features/Property/Endpoints/`
   - `Models/` - Create DTOs (PropertyDetailResponse, CreatePropertyRequest, etc.)
2. Update services to import entities from `RealinApi.Data.Entities`
3. Create `CatalogModule.cs`
4. **Keep entities in `Data/Entities/`** - do NOT move them

### Step 4: Create Engagement Module
1. Create `Modules/Engagement/` folder structure:
   - `Services/` - New services for Favorites, Leads, Inquiries
   - `Endpoints/` - New endpoints for engagement features
   - `Models/` - Create DTOs
2. Add new entities to `Data/Entities/` (Favorite, Lead, Inquiry if not existing)
3. Update `AppDbContext` with new entity configurations
4. Create `EngagementModule.cs`

### Step 5: Update Program.cs
1. Update `Program.cs` to use module registration pattern:
   ```csharp
   builder.Services.AddIdentityModule(builder.Configuration);
   builder.Services.AddCatalogModule();
   builder.Services.AddEngagementModule();
   
   app.MapIdentityEndpoints();
   app.MapCatalogEndpoints();
   app.MapEngagementEndpoints();
   ```
2. Remove old feature registrations

### Step 6: Cleanup
1. Remove old `Features/` folder after verifying all modules work
2. Test all endpoints
3. Run EF Core migrations if new entities were added

---

## Testing Strategy

```
RealinApi.Tests/
├── Identity/
│   ├── AuthServiceTests.cs
│   ├── JwtServiceTests.cs
│   └── UserServiceTests.cs
├── Catalog/
│   ├── PropertyServiceTests.cs
│   └── ProjectServiceTests.cs
└── Engagement/
    ├── FavoriteServiceTests.cs
    └── LeadServiceTests.cs
```

---

## Future Considerations

### Potential Module Additions
1. **Search Module**: Elasticsearch integration for advanced property search
2. **Analytics Module**: User behavior and market analytics
3. **Notification Module**: Push notifications and alerts
4. **Payment Module**: Subscription and transaction handling

### Migration to Microservices
If scaling requires it, modules can be extracted to microservices:
1. Each module already has clear boundaries
2. Shared Kernel DTOs become API contracts
3. Database can be split per module
4. Communication moves to HTTP/gRPC/Message Queue

---

## Summary

This modular monolith architecture provides:

| Benefit | Description |
|---------|-------------|
| **Clear Boundaries** | Each domain has its own module with explicit responsibilities |
| **Maintainability** | Changes are isolated within modules |
| **Testability** | Modules can be tested independently |
| **Scalability Path** | Can evolve to microservices when needed |
| **Simplicity** | Single deployment, shared database, no network overhead |
