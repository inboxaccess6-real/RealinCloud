# Code Migration Plan: Unified Solution with Clean Architecture

## Purpose

This document plans the reorganization of the existing `RealinServer/RealinApi/` monolith into a **unified Clean Architecture solution** that houses both the API backend (serving iOS/Android mobile apps) and a Blazor WebAssembly admin panel (used by internal admins and agents).

### Key Architecture Decisions

1. **One Solution** — API and Admin Panel live in the same solution, sharing Domain, Application, and Infrastructure layers.
2. **Blazor WASM** (not Server) — The admin panel runs in the browser and calls the API over HTTP. This avoids maintaining two separate server-side applications.
3. **Agents use the admin panel** — Agents log in with restricted module access to upload/manage their properties. This is not just an internal tool.
4. **Single API backend** — `RealEstate.Api` serves both mobile apps and the WASM admin panel. No duplicate backend.
5. **Shared database** — One PostgreSQL database, one schema (`realin`), one set of migrations.

---

## 1. Target Solution Structure

```
RealinCloud/
└── RealEstate.sln
    │
    ├── src/
    │   ├── RealEstate.Domain/            (Class Library - pure business logic)
    │   ├── RealEstate.Application/       (Class Library - use cases, DTOs, interfaces)
    │   ├── RealEstate.Infrastructure/    (Class Library - EF Core, S3, auth services)
    │   ├── RealEstate.Api/               (ASP.NET Core Minimal API - was RealinServer)
    │   └── RealEstate.Admin/             (Blazor WASM - admin panel)
    │
    └── tests/
        ├── RealEstate.Domain.Tests/      (xUnit)
        ├── RealEstate.Application.Tests/ (xUnit)
        └── RealEstate.Infrastructure.Tests/ (xUnit)
```

### Project Dependencies

```
Domain              → (nothing)
Application         → Domain
Infrastructure      → Application
Api                 → Application, Infrastructure
Admin               → Application (DTOs only — no Infrastructure, no DB access)
```

### What Each Project Contains

| Project | Responsibility | Depends On |
|---------|---------------|------------|
| **RealEstate.Domain** | Entities, enums, value objects, domain exceptions. Zero NuGet packages. | Nothing |
| **RealEstate.Application** | Repository interfaces, service interfaces, MediatR commands/queries, DTOs, FluentValidation validators, pipeline behaviors. | Domain |
| **RealEstate.Infrastructure** | AppDbContext, EF configurations, repository implementations, JwtService, OtpService, PermissionService, S3 storage, Google/Apple OAuth, email/SMS services, migrations. | Application |
| **RealEstate.Api** | Minimal API endpoint definitions (`MapGet`, `MapPost`), Program.cs, DI wiring, CORS, middleware. Thin HTTP layer — no business logic. | Application, Infrastructure |
| **RealEstate.Admin** | Blazor WASM pages, components, layouts, HttpClient-based API services. References Application for shared DTOs only. | Application (DTOs only) |

### How It All Connects

```
┌─────────────────┐     ┌──────────────────┐
│  iOS/Android     │     │  Blazor WASM      │
│  Mobile Apps     │     │  Admin Panel      │
│                  │     │  (static files)   │
└────────┬─────── ┘     └────────┬──────────┘
         │  HTTP/JSON            │  HTTP/JSON
         │                       │
         └───────────┬───────────┘
                     │
              ┌──────▼──────┐
              │ RealEstate  │
              │    .Api     │  ← Minimal API endpoints
              └──────┬──────┘
                     │
              ┌──────▼──────┐
              │ RealEstate  │
              │ .Application│  ← MediatR handlers, business logic
              └──────┬──────┘
                     │
         ┌───────────┼───────────┐
         ▼                       ▼
┌────────────────┐     ┌────────────────┐
│  RealEstate    │     │  RealEstate    │
│   .Domain      │     │.Infrastructure │
│  (entities)    │     │  (EF Core, S3) │
└────────────────┘     └────────┬───────┘
                                │
                         ┌──────▼──────┐
                         │ PostgreSQL  │
                         │  (realin)   │
                         └─────────────┘
```

---

## 2. Source Inventory (Current RealinServer)

```
RealinApi/                                          → TARGET PROJECT
├── Program.cs                                      → Api
├── Common/
│   ├── ApiResponse.cs                              → Application/Common
│   └── ValidationExtensions.cs                     → Domain/ValueObjects
├── Data/
│   ├── AppDbContext.cs                (606 lines)  → Infrastructure/Persistence
│   ├── AppDbContextFactory.cs                      → Infrastructure/Persistence
│   ├── SeedPermissions.cs                          → Infrastructure/Persistence
│   └── Entities/
│       ├── User.cs                                 → Domain/Entities
│       ├── Role.cs                                 → Domain/Entities
│       ├── RoleType.cs                             → Domain/Enums
│       ├── Module.cs                               → Domain/Entities
│       ├── RolePermission.cs                       → Domain/Entities
│       ├── AuthProvider.cs                         → Domain/Enums
│       ├── RefreshToken.cs                         → Domain/Entities
│       ├── OtpSession.cs                           → Domain/Entities
│       ├── OtpDeliveryMethod.cs                    → Domain/Enums
│       ├── Property.cs                             → Domain/Entities
│       ├── Agent.cs                                → Domain/Entities
│       ├── Builder.cs                              → Domain/Entities
│       ├── Project.cs                              → Domain/Entities
│       ├── Media.cs                                → Domain/Entities
│       ├── Favorite.cs                             → Domain/Entities
│       ├── Lead.cs                                 → Domain/Entities
│       └── Inquiry.cs                              → Domain/Entities
├── Features/
│   ├── Auth/
│   │   ├── AuthEndpoints.cs                        → Api/Endpoints
│   │   ├── AuthService.cs                          → Application/Features/Auth
│   │   └── Models/AuthModels.cs                    → Application/DTOs
│   ├── Admin/
│   │   ├── UserEndpoints.cs                        → Api/Endpoints
│   │   ├── UserService.cs                          → Application/Features/Users
│   │   ├── RoleEndpoints.cs                        → Api/Endpoints
│   │   ├── RoleService.cs                          → Application/Features/Roles
│   │   ├── ModuleEndpoints.cs                      → Api/Endpoints
│   │   ├── ModuleService.cs                        → Application/Features/Modules
│   │   └── Models/
│   │       ├── UserModels.cs                       → Application/DTOs
│   │       ├── RoleModels.cs                       → Application/DTOs
│   │       └── ModuleModels.cs                     → Application/DTOs
│   └── Property/
│       ├── PropertyEndpoints.cs                    → Api/Endpoints
│       ├── PropertyService.cs                      → Application/Features/Properties
│       ├── AgentEndpoints.cs                       → Api/Endpoints
│       ├── AgentService.cs                         → Application/Features/Agents
│       ├── BuilderEndpoints.cs                     → Api/Endpoints
│       ├── BuilderService.cs                       → Application/Features/Builders
│       ├── ProjectEndpoints.cs                     → Api/Endpoints
│       ├── ProjectService.cs                       → Application/Features/Projects
│       └── Models/
│           ├── PropertyModels.cs                   → Application/DTOs
│           ├── AgentModels.cs                      → Application/DTOs
│           ├── BuilderModels.cs                    → Application/DTOs
│           ├── ProjectModels.cs                    → Application/DTOs
│           └── SharedModels.cs                     → Application/DTOs
└── Infrastructure/
    ├── Authentication/
    │   ├── JwtService.cs                           → Infrastructure/Identity
    │   ├── OtpService.cs                           → Infrastructure/Identity
    │   └── TokenResult.cs                          → Infrastructure/Identity
    ├── Authorization/
    │   └── PermissionService.cs                    → Infrastructure/Identity
    ├── ExternalServices/
    │   ├── GoogleAuthService.cs                    → Infrastructure/ExternalAuth
    │   └── AppleAuthService.cs                     → Infrastructure/ExternalAuth
    └── Messaging/
        ├── EmailService.cs                         → Infrastructure/Messaging
        └── SmsService.cs                           → Infrastructure/Messaging
```

**Key difference from the previous plan**: Nothing is dropped. Every file moves to a layer in the shared solution. The API endpoints stay (they serve mobile apps AND the admin WASM panel). External auth services stay (mobile apps use Google/Apple login). Messaging stays (SMS/email OTP for mobile users).

---

## 3. Detailed Migration Mapping

### Legend

- **MOVE** = Move file to new location, update namespace. Minimal code changes.
- **MOVE+ADAPT** = Move file, update namespace, and modify code (add new fields, clean up dependencies).
- **REWRITE** = Significant restructuring of the code to fit Clean Architecture patterns.
- **NEW** = Does not exist in RealinServer, must be created.

---

### 3.1 Domain Layer — `RealEstate.Domain/`

The Domain project has **zero NuGet dependencies**. Entities are pure POCOs. Navigation properties are kept (EF Core can map them without attributes), but no EF-specific annotations.

#### Entities

| Source | Destination | Action | Changes |
|--------|-------------|--------|---------|
| `Data/Entities/User.cs` | `Domain/Entities/User.cs` | MOVE+ADAPT | Add: IsBlocked, IsBlacklisted, BlacklistReason, BlockedBy, BlockedAt, IsDeleted, DeletedAt, DeletedBy |
| `Data/Entities/Role.cs` | `Domain/Entities/Role.cs` | MOVE | Namespace change only |
| `Data/Entities/Module.cs` | `Domain/Entities/Module.cs` | MOVE | Namespace change only |
| `Data/Entities/RolePermission.cs` | `Domain/Entities/RolePermission.cs` | MOVE+ADAPT | Add: CanExport (optional) |
| `Data/Entities/RefreshToken.cs` | `Domain/Entities/RefreshToken.cs` | MOVE | Namespace change only |
| `Data/Entities/OtpSession.cs` | `Domain/Entities/OtpSession.cs` | MOVE | Namespace change only |
| `Data/Entities/Property.cs` | `Domain/Entities/Property.cs` | MOVE+ADAPT | Add: ApprovalStatus, RejectionReason, ReviewedBy, ReviewedAt, SubmittedAt, IsFlagged, FlagReason, IsDeleted, DeletedAt, DeletedBy |
| `Data/Entities/Agent.cs` | `Domain/Entities/Agent.cs` | MOVE+ADAPT | Add: Status, VerificationNotes, VerifiedBy, VerifiedAt, IdProofUrl, CompanyDetails, IsBlocked, IsBlacklisted, BlacklistReason, BlockedBy, BlockedAt, IsDeleted, DeletedAt |
| `Data/Entities/Builder.cs` | `Domain/Entities/Builder.cs` | MOVE+ADAPT | Add: IsBlocked, IsBlacklisted, BlacklistReason, BlockedBy, BlockedAt, IsDeleted, DeletedAt, UpdatedAt |
| `Data/Entities/Project.cs` | `Domain/Entities/Project.cs` | MOVE+ADAPT | Add: IsBlocked, IsBlacklisted, BlacklistReason, BlockedBy, BlockedAt, IsDeleted, DeletedAt, UpdatedAt |
| `Data/Entities/Media.cs` | `Domain/Entities/Media.cs` | MOVE | Namespace change only |
| `Data/Entities/Favorite.cs` | `Domain/Entities/Favorite.cs` | MOVE | Namespace change only |
| `Data/Entities/Lead.cs` | `Domain/Entities/Lead.cs` | MOVE+ADAPT | Extract LeadStatus enum to separate file |
| `Data/Entities/Inquiry.cs` | `Domain/Entities/Inquiry.cs` | MOVE+ADAPT | Extract InquiryStatus enum to separate file |
| (new) | `Domain/Entities/AuditLog.cs` | NEW | id, action, entity_type, entity_id, performed_by, details (jsonb), ip_address, created_at |

#### Enums

| Source | Destination | Action |
|--------|-------------|--------|
| `Data/Entities/RoleType.cs` | `Domain/Enums/RoleType.cs` | MOVE (enum + extension methods) |
| `Data/Entities/AuthProvider.cs` | `Domain/Enums/AuthProvider.cs` | MOVE |
| `Data/Entities/OtpDeliveryMethod.cs` | `Domain/Enums/OtpDeliveryMethod.cs` | MOVE |
| (from Lead.cs) | `Domain/Enums/LeadStatus.cs` | MOVE (extract to own file) |
| (from Inquiry.cs) | `Domain/Enums/InquiryStatus.cs` | MOVE (extract to own file) |
| (new) | `Domain/Enums/AgentStatus.cs` | NEW — Pending, Approved, Rejected, Suspended, Blacklisted |
| (new) | `Domain/Enums/PropertyApprovalStatus.cs` | NEW — Draft, Submitted, UnderReview, Approved, Rejected |

#### Value Objects

| Destination | Action | Notes |
|-------------|--------|-------|
| `Domain/ValueObjects/Email.cs` | NEW | Wraps email string with validation (from `ValidationExtensions.IsValidEmail`) |
| `Domain/ValueObjects/PhoneNumber.cs` | NEW | Wraps phone string with validation (from `ValidationExtensions.IsValidPhoneNumber`) |
| `Domain/ValueObjects/Money.cs` | NEW | Price + Currency pair |

#### Exceptions

| Destination | Action |
|-------------|--------|
| `Domain/Exceptions/DomainException.cs` | NEW — Base exception |
| `Domain/Exceptions/NotFoundException.cs` | NEW — Entity not found |
| `Domain/Exceptions/BusinessRuleException.cs` | NEW — Business rule violation |
| `Domain/Exceptions/ForbiddenException.cs` | NEW — Permission denied |

---

### 3.2 Application Layer — `RealEstate.Application/`

NuGet packages: `MediatR`, `FluentValidation`

#### Interfaces

| Source | Destination | Action | Notes |
|--------|-------------|--------|-------|
| (from PropertyService) | `Interfaces/IPropertyRepository.cs` | NEW | GetById, GetPaged, Create, Update, SoftDelete, GetByApprovalStatus |
| (from AgentService) | `Interfaces/IAgentRepository.cs` | NEW | GetById, GetByUserId, GetPaged, Create, Update, GetByStatus |
| (from BuilderService) | `Interfaces/IBuilderRepository.cs` | NEW | GetById, GetPaged, Create, Update |
| (from ProjectService) | `Interfaces/IProjectRepository.cs` | NEW | GetById, GetPaged, GetByBuilderId, Create, Update |
| (from UserService) | `Interfaces/IUserRepository.cs` | NEW | GetById, GetPaged, Update, GetByEmail, GetByPhone |
| (from RoleService) | `Interfaces/IRoleRepository.cs` | NEW | GetAll, GetById, Create, Update, Delete |
| (from ModuleService) | `Interfaces/IModuleRepository.cs` | NEW | GetAll, GetById, Create, Update, Delete |
| (new) | `Interfaces/IAuditLogRepository.cs` | NEW | Create, GetPaged, GetByEntity, GetByUser |
| (from JwtService) | `Interfaces/IJwtService.cs` | NEW | Extract interface from existing class |
| (from OtpService) | `Interfaces/IOtpService.cs` | NEW | Extract interface from existing class |
| (from PermissionService) | `Interfaces/IPermissionService.cs` | NEW | Extract interface from existing class |
| (new) | `Interfaces/ICurrentUserService.cs` | NEW | Get current user from HTTP context (userId, role) |
| (new) | `Interfaces/IFileStorageService.cs` | NEW | Upload, Delete, GetPresignedUrl (S3 abstraction) |
| (from GoogleAuthService) | `Interfaces/IGoogleAuthService.cs` | NEW | Extract interface |
| (from AppleAuthService) | `Interfaces/IAppleAuthService.cs` | NEW | Extract interface |
| (from EmailService) | `Interfaces/IEmailService.cs` | NEW | Extract interface |
| (from SmsService) | `Interfaces/ISmsService.cs` | NEW | Extract interface |

#### DTOs (Shared between API and Admin WASM)

This is the critical shared layer. The API serializes these as JSON responses. The Admin WASM app deserializes them as typed C# objects. Same types, zero duplication.

| Source | Destination | Action | Notes |
|--------|-------------|--------|-------|
| `Features/Auth/Models/AuthModels.cs` | `DTOs/Auth/AuthDtos.cs` | MOVE+ADAPT | GoogleLoginRequest, AppleLoginRequest, OtpRequestRequest, OtpVerifyRequest, AuthResponse, UserInfo |
| `Features/Admin/Models/UserModels.cs` | `DTOs/Users/UserDtos.cs` | MOVE+ADAPT | Add moderation fields to UserResponse |
| `Features/Admin/Models/RoleModels.cs` | `DTOs/Roles/RoleDtos.cs` | MOVE+ADAPT | Add permission details to RoleResponse |
| `Features/Admin/Models/ModuleModels.cs` | `DTOs/Modules/ModuleDtos.cs` | MOVE | Namespace change |
| `Features/Property/Models/PropertyModels.cs` | `DTOs/Properties/PropertyDtos.cs` | MOVE+ADAPT | Add approval workflow fields |
| `Features/Property/Models/AgentModels.cs` | `DTOs/Agents/AgentDtos.cs` | MOVE+ADAPT | Add verification status fields |
| `Features/Property/Models/BuilderModels.cs` | `DTOs/Builders/BuilderDtos.cs` | MOVE+ADAPT | Add moderation fields |
| `Features/Property/Models/ProjectModels.cs` | `DTOs/Projects/ProjectDtos.cs` | MOVE+ADAPT | Add moderation fields |
| `Features/Property/Models/SharedModels.cs` | `DTOs/Common/SummaryDtos.cs` | MOVE | AgentSummary, BuilderSummary, etc. |
| (new) | `DTOs/AuditLogs/AuditLogDtos.cs` | NEW | AuditLogResponse, AuditLogFilter |
| (new) | `DTOs/Dashboard/DashboardDtos.cs` | NEW | DashboardMetrics, TrendData |
| (new) | `DTOs/Common/PagedResult.cs` | NEW | Generic `PagedResult<T>` for all list endpoints |

#### Features (MediatR Commands & Queries)

Each existing service method becomes a MediatR request + handler. Each new admin operation is also a MediatR request.

**Properties**

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `PropertyService.GetPropertyByIdAsync` | `Features/Properties/Queries/GetPropertyById.cs` | REWRITE as IRequest<PropertyResponse> |
| `PropertyService.GetPropertiesAsync` | `Features/Properties/Queries/GetProperties.cs` | REWRITE as IRequest<PagedResult<PropertyResponse>> |
| `PropertyService.CreatePropertyAsync` | `Features/Properties/Commands/CreateProperty.cs` | REWRITE as IRequest<PropertyResponse> |
| `PropertyService.UpdatePropertyAsync` | `Features/Properties/Commands/UpdateProperty.cs` | REWRITE |
| `PropertyService.DeletePropertyAsync` | `Features/Properties/Commands/DeleteProperty.cs` | REWRITE as soft delete |
| (new) | `Features/Properties/Commands/SubmitPropertyForReview.cs` | NEW — Agent submits for approval |
| (new) | `Features/Properties/Commands/ApproveProperty.cs` | NEW — Admin approves |
| (new) | `Features/Properties/Commands/RejectProperty.cs` | NEW — Admin rejects with reason |
| (new) | `Features/Properties/Commands/FlagProperty.cs` | NEW — Flag inappropriate content |
| (new) | `Features/Properties/Queries/GetPendingApprovals.cs` | NEW — Approval queue |

**Agents**

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `AgentService.GetAgentByIdAsync` | `Features/Agents/Queries/GetAgentById.cs` | REWRITE |
| `AgentService.GetAgentByUserIdAsync` | `Features/Agents/Queries/GetAgentByUserId.cs` | REWRITE |
| `AgentService.GetAgentsAsync` | `Features/Agents/Queries/GetAgents.cs` | REWRITE with status filters |
| `AgentService.CreateAgentAsync` | `Features/Agents/Commands/CreateAgent.cs` | REWRITE |
| `AgentService.UpdateAgentAsync` | `Features/Agents/Commands/UpdateAgent.cs` | REWRITE |
| (new) | `Features/Agents/Commands/ApproveAgent.cs` | NEW — Verification workflow |
| (new) | `Features/Agents/Commands/RejectAgent.cs` | NEW |
| (new) | `Features/Agents/Commands/SuspendAgent.cs` | NEW |
| (new) | `Features/Agents/Commands/BlacklistAgent.cs` | NEW |
| (new) | `Features/Agents/Queries/GetPendingAgentVerifications.cs` | NEW |

**Users**

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `UserService.GetAllUsersAsync` | `Features/Users/Queries/GetUsers.cs` | REWRITE with filters, pagination |
| `UserService.GetUserByIdAsync` | `Features/Users/Queries/GetUserById.cs` | REWRITE |
| `UserService.UpdateUserAsync` | `Features/Users/Commands/UpdateUser.cs` | REWRITE |
| `UserService.DeleteUserAsync` | `Features/Users/Commands/DeleteUser.cs` | REWRITE as soft delete |
| (new) | `Features/Users/Commands/BlockUser.cs` | NEW |
| (new) | `Features/Users/Commands/UnblockUser.cs` | NEW |
| (new) | `Features/Users/Commands/BlacklistUser.cs` | NEW |

**Roles**

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `RoleService.GetAllRolesAsync` | `Features/Roles/Queries/GetRoles.cs` | REWRITE |
| `RoleService.GetRoleByIdAsync` | `Features/Roles/Queries/GetRoleById.cs` | REWRITE |
| `RoleService.CreateRoleAsync` | `Features/Roles/Commands/CreateRole.cs` | REWRITE |
| `RoleService.UpdateRoleAsync` | `Features/Roles/Commands/UpdateRole.cs` | REWRITE |
| `RoleService.DeleteRoleAsync` | `Features/Roles/Commands/DeleteRole.cs` | REWRITE |
| (new) | `Features/Roles/Commands/AssignPermissions.cs` | NEW — Manage role-module permissions |

**Builders**

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `BuilderService.GetBuilderByIdAsync` | `Features/Builders/Queries/GetBuilderById.cs` | REWRITE |
| `BuilderService.GetBuildersAsync` | `Features/Builders/Queries/GetBuilders.cs` | REWRITE |
| `BuilderService.CreateBuilderAsync` | `Features/Builders/Commands/CreateBuilder.cs` | REWRITE |
| `BuilderService.UpdateBuilderAsync` | `Features/Builders/Commands/UpdateBuilder.cs` | REWRITE |
| (new) | `Features/Builders/Commands/BlockBuilder.cs` | NEW |
| (new) | `Features/Builders/Commands/BlacklistBuilder.cs` | NEW |

**Projects** (not yet in source as feature, but ProjectService exists)

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `ProjectService.GetProjectByIdAsync` | `Features/Projects/Queries/GetProjectById.cs` | REWRITE |
| `ProjectService.GetProjectsAsync` | `Features/Projects/Queries/GetProjects.cs` | REWRITE |
| `ProjectService.CreateProjectAsync` | `Features/Projects/Commands/CreateProject.cs` | REWRITE |
| `ProjectService.UpdateProjectAsync` | `Features/Projects/Commands/UpdateProject.cs` | REWRITE |

**Auth** (existing flows preserved for mobile apps)

| Source Method | Destination | Action |
|---------------|-------------|--------|
| `AuthService.GoogleLoginAsync` | `Features/Auth/Commands/GoogleLogin.cs` | REWRITE |
| `AuthService.AppleLoginAsync` | `Features/Auth/Commands/AppleLogin.cs` | REWRITE |
| `AuthService.RequestOtpAsync` | `Features/Auth/Commands/RequestOtp.cs` | REWRITE |
| `AuthService.VerifyOtpAsync` | `Features/Auth/Commands/VerifyOtp.cs` | REWRITE |
| `AuthService.RefreshTokenAsync` | `Features/Auth/Commands/RefreshToken.cs` | REWRITE |

**AuditLogs** (entirely new)

| Destination | Action |
|-------------|--------|
| `Features/AuditLogs/Queries/GetAuditLogs.cs` | NEW — Paginated with filters |
| `Features/AuditLogs/Queries/GetEntityAuditTrail.cs` | NEW — All logs for a specific entity |

**Dashboard** (entirely new)

| Destination | Action |
|-------------|--------|
| `Features/Dashboard/Queries/GetDashboardMetrics.cs` | NEW — Aggregation counts |
| `Features/Dashboard/Queries/GetActivityTrends.cs` | NEW — Time-series data |

#### Common / Cross-Cutting

| Source | Destination | Action | Notes |
|--------|-------------|--------|-------|
| `Common/ApiResponse.cs` | `Common/ApiResponse.cs` | MOVE | Keep for API responses. Still useful. |
| (new) | `Common/Behaviors/ValidationBehavior.cs` | NEW | MediatR pipeline: runs FluentValidation before handler |
| (new) | `Common/Behaviors/AuditLogBehavior.cs` | NEW | MediatR pipeline: auto-logs commands marked with `IAuditableCommand` |
| (new) | `Common/Mappings/MappingExtensions.cs` | NEW | Entity ↔ DTO mapping extension methods |

#### Services

| Destination | Action | Notes |
|-------------|--------|-------|
| `Services/DashboardService.cs` | NEW | Could also be MediatR handlers; either approach works |

---

### 3.3 Infrastructure Layer — `RealEstate.Infrastructure/`

NuGet packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `AWSSDK.S3`

#### Persistence

| Source | Destination | Action | Notes |
|--------|-------------|--------|-------|
| `Data/AppDbContext.cs` | `Persistence/AppDbContext.cs` | REWRITE | Same `realin` schema. Break 606-line OnModelCreating into `IEntityTypeConfiguration<T>` files. Add global query filters for soft delete. Add new entity DbSets (AuditLog). |
| `Data/AppDbContextFactory.cs` | `Persistence/AppDbContextFactory.cs` | MOVE | Needed for `dotnet ef` CLI. Migrations move here. |
| `Data/SeedPermissions.cs` | `Persistence/SeedData.cs` | MOVE+ADAPT | Extend with new modules (BLACKLIST, AUDIT_LOGS, REPORTS, APPROVALS) |

**Entity Configurations** — Each extracted from the monolithic `OnModelCreating`:

| Destination | Action | Lines from Source |
|-------------|--------|-------------------|
| `Persistence/Configurations/UserConfiguration.cs` | NEW (extract) | AppDbContext lines 39-67 |
| `Persistence/Configurations/RefreshTokenConfiguration.cs` | NEW (extract) | lines 70-89 |
| `Persistence/Configurations/OtpSessionConfiguration.cs` | NEW (extract) | lines 92-115 |
| `Persistence/Configurations/RoleConfiguration.cs` | NEW (extract) | lines 118-132 |
| `Persistence/Configurations/ModuleConfiguration.cs` | NEW (extract) | lines 135-149 |
| `Persistence/Configurations/RolePermissionConfiguration.cs` | NEW (extract) | lines 152-180 |
| `Persistence/Configurations/BuilderConfiguration.cs` | NEW (extract) | lines 187-211 + new columns |
| `Persistence/Configurations/ProjectConfiguration.cs` | NEW (extract) | lines 214-252 + new columns |
| `Persistence/Configurations/PropertyConfiguration.cs` | NEW (extract) | lines 255-324 + new columns |
| `Persistence/Configurations/MediaConfiguration.cs` | NEW (extract) | lines 327-344 |
| `Persistence/Configurations/AgentConfiguration.cs` | NEW (extract) | lines 347-367 + new columns |
| `Persistence/Configurations/FavoriteConfiguration.cs` | NEW (extract) | lines 374-396 |
| `Persistence/Configurations/LeadConfiguration.cs` | NEW (extract) | lines 399-435 |
| `Persistence/Configurations/InquiryConfiguration.cs` | NEW (extract) | lines 438-468 |
| `Persistence/Configurations/AuditLogConfiguration.cs` | NEW | New table |

**Repositories** — Implement interfaces from Application layer:

| Destination | Implements | Action |
|-------------|-----------|--------|
| `Persistence/Repositories/PropertyRepository.cs` | `IPropertyRepository` | NEW |
| `Persistence/Repositories/AgentRepository.cs` | `IAgentRepository` | NEW |
| `Persistence/Repositories/UserRepository.cs` | `IUserRepository` | NEW |
| `Persistence/Repositories/RoleRepository.cs` | `IRoleRepository` | NEW |
| `Persistence/Repositories/BuilderRepository.cs` | `IBuilderRepository` | NEW |
| `Persistence/Repositories/ProjectRepository.cs` | `IProjectRepository` | NEW |
| `Persistence/Repositories/ModuleRepository.cs` | `IModuleRepository` | NEW |
| `Persistence/Repositories/AuditLogRepository.cs` | `IAuditLogRepository` | NEW |

Business logic currently embedded in services (e.g., "check for duplicate agent", "validate agent exists for property") moves into the MediatR handlers in Application. Repositories are thin data-access wrappers.

#### Identity

| Source | Destination | Action | Notes |
|--------|-------------|--------|-------|
| `Infrastructure/Authentication/JwtService.cs` | `Identity/JwtService.cs` | MOVE | Implements IJwtService |
| `Infrastructure/Authentication/OtpService.cs` | `Identity/OtpService.cs` | MOVE | Implements IOtpService |
| `Infrastructure/Authentication/TokenResult.cs` | `Identity/TokenResult.cs` | MOVE | Record type |
| `Infrastructure/Authorization/PermissionService.cs` | `Identity/PermissionService.cs` | MOVE | Implements IPermissionService |
| (new) | `Identity/CurrentUserService.cs` | NEW | Implements ICurrentUserService, reads from HttpContext claims |

#### External Auth

| Source | Destination | Action |
|--------|-------------|--------|
| `Infrastructure/ExternalServices/GoogleAuthService.cs` | `ExternalAuth/GoogleAuthService.cs` | MOVE — implements IGoogleAuthService |
| `Infrastructure/ExternalServices/AppleAuthService.cs` | `ExternalAuth/AppleAuthService.cs` | MOVE — implements IAppleAuthService |

#### Storage

| Destination | Action |
|-------------|--------|
| `Storage/S3StorageService.cs` | NEW — Implements IFileStorageService |

#### Messaging

| Source | Destination | Action |
|--------|-------------|--------|
| `Infrastructure/Messaging/EmailService.cs` | `Messaging/EmailService.cs` | MOVE — implements IEmailService |
| `Infrastructure/Messaging/SmsService.cs` | `Messaging/SmsService.cs` | MOVE — implements ISmsService |

#### Search (Future)

Empty placeholder for Elasticsearch.

#### DI Registration

| Destination | Action | Notes |
|-------------|--------|-------|
| `DependencyInjection.cs` | NEW | `AddInfrastructure(IServiceCollection, IConfiguration)` extension method. Registers DbContext, all repositories, all services. |

---

### 3.4 API Layer — `RealEstate.Api/`

Thin HTTP layer. No business logic. Each endpoint receives an HTTP request, dispatches a MediatR command/query, and returns the result.

| Source | Destination | Action | Notes |
|--------|-------------|--------|-------|
| `Features/Auth/AuthEndpoints.cs` | `Endpoints/AuthEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR commands |
| `Features/Admin/UserEndpoints.cs` | `Endpoints/Admin/UserEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| `Features/Admin/RoleEndpoints.cs` | `Endpoints/Admin/RoleEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| `Features/Admin/ModuleEndpoints.cs` | `Endpoints/Admin/ModuleEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| `Features/Property/PropertyEndpoints.cs` | `Endpoints/PropertyEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| `Features/Property/AgentEndpoints.cs` | `Endpoints/AgentEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| `Features/Property/BuilderEndpoints.cs` | `Endpoints/BuilderEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| `Features/Property/ProjectEndpoints.cs` | `Endpoints/ProjectEndpoints.cs` | MOVE+ADAPT | Rewrite to dispatch MediatR |
| (new) | `Endpoints/Admin/ApprovalEndpoints.cs` | NEW | Property/Agent approval workflows |
| (new) | `Endpoints/Admin/ModerationEndpoints.cs` | NEW | Block/blacklist operations |
| (new) | `Endpoints/Admin/AuditLogEndpoints.cs` | NEW | Audit log queries |
| (new) | `Endpoints/Admin/DashboardEndpoints.cs` | NEW | Dashboard metrics |
| `Program.cs` | `Program.cs` | REWRITE | DI wiring calls `AddApplication()` + `AddInfrastructure()`. CORS configured to allow Admin WASM origin. Serves WASM static files (or separate deployment). |

#### Api Project Structure

```
RealEstate.Api/
├── Program.cs
├── Endpoints/
│   ├── AuthEndpoints.cs
│   ├── PropertyEndpoints.cs
│   ├── AgentEndpoints.cs
│   ├── BuilderEndpoints.cs
│   ├── ProjectEndpoints.cs
│   └── Admin/
│       ├── UserEndpoints.cs
│       ├── RoleEndpoints.cs
│       ├── ModuleEndpoints.cs
│       ├── ApprovalEndpoints.cs
│       ├── ModerationEndpoints.cs
│       ├── AuditLogEndpoints.cs
│       └── DashboardEndpoints.cs
└── Middleware/
    └── ExceptionHandlingMiddleware.cs    (NEW - maps domain exceptions to HTTP status codes)
```

---

### 3.5 Admin Layer — `RealEstate.Admin/` (Blazor WASM)

This project references **Application only** (for shared DTOs). It does NOT reference Infrastructure. It does NOT have database access. It communicates with the API via `HttpClient`.

```
RealEstate.Admin/
├── Program.cs                            (WASM startup, HttpClient config, auth setup)
├── wwwroot/
│   └── index.html
├── Services/
│   ├── ApiClient.cs                      (Base HttpClient wrapper with auth headers)
│   ├── PropertyApiService.cs             (Calls /api/properties/*)
│   ├── AgentApiService.cs                (Calls /api/agents/*)
│   ├── UserApiService.cs                 (Calls /api/admin/users/*)
│   ├── RoleApiService.cs                 (Calls /api/admin/roles/*)
│   ├── BuilderApiService.cs              (Calls /api/builders/*)
│   ├── ProjectApiService.cs              (Calls /api/projects/*)
│   ├── ApprovalApiService.cs             (Calls /api/admin/approvals/*)
│   ├── AuditLogApiService.cs             (Calls /api/admin/audit-logs/*)
│   └── DashboardApiService.cs            (Calls /api/admin/dashboard/*)
├── Pages/
│   ├── Dashboard/
│   ├── Users/
│   ├── Agents/
│   ├── Properties/
│   ├── Roles/
│   ├── Approvals/
│   ├── Analytics/
│   ├── AuditLogs/
│   └── Settings/
├── Components/
├── Layouts/
└── Auth/
    └── (JWT token storage, AuthStateProvider, etc.)
```

The DTOs used by `PropertyApiService.cs` (e.g., `CreatePropertyRequest`, `PropertyResponse`) come directly from `RealEstate.Application/DTOs/`. Same C# types used by the API to serialize and by the WASM app to deserialize. Zero duplication.

---

## 4. Migration Execution Order

### Phase 0: Prerequisites

```
0.1  Apply database schema changes (see 01-Database-Tables-and-Migration-Plan.md)
     - Run migrations in the CURRENT RealinServer project
     - Verify schema changes on dev database
0.2  Verify existing API still works after schema changes (all new columns have defaults)
```

### Phase 1: Create New Solution Structure

```
1.1  Create RealEstate.sln with 5 src projects + 3 test projects
1.2  Set up project references (dependency graph)
1.3  Add NuGet packages to each project
1.4  Verify: dotnet build succeeds (empty projects)
```

### Phase 2: Domain Layer (bottom-up)

```
2.1  Move all 14 entity classes into Domain/Entities/, update namespaces
2.2  Add new fields to entities (moderation, approval, soft delete) per DB plan
2.3  Create AuditLog entity
2.4  Move 3 enums + extract 2 embedded enums + create 2 new enums → Domain/Enums/
2.5  Create value objects (Email, PhoneNumber, Money)
2.6  Create domain exceptions (4 files)
2.7  Verify: Domain builds with zero NuGet packages
```

### Phase 3: Application Layer

```
3.1  Create all repository interfaces (8 files)
3.2  Create all service interfaces (IJwtService, IOtpService, IPermissionService,
     ICurrentUserService, IFileStorageService, IGoogleAuthService, IAppleAuthService,
     IEmailService, ISmsService)
3.3  Move + adapt all DTOs (12 files)
3.4  Create Common (ApiResponse, PagedResult, ValidationBehavior, AuditLogBehavior,
     MappingExtensions)
3.5  Create MediatR features — start with one vertical slice to validate the pattern:
     - Features/Users/Queries/GetUsers.cs (Query + Handler + Validator)
3.6  Build out ALL remaining features (Auth, Properties, Agents, Builders, Projects,
     Roles, AuditLogs, Dashboard)
3.7  Create Application DI extension: AddApplication(IServiceCollection)
3.8  Verify: Application builds
```

### Phase 4: Infrastructure Layer

```
4.1  Create AppDbContext + AppDbContextFactory
4.2  Create all 15 IEntityTypeConfiguration files (extracted from monolithic OnModelCreating)
4.3  Add global query filters for soft delete
4.4  Implement all 8 repository classes
4.5  Move JwtService, OtpService, PermissionService → Identity/
4.6  Create CurrentUserService
4.7  Move GoogleAuthService, AppleAuthService → ExternalAuth/
4.8  Move EmailService, SmsService → Messaging/
4.9  Create S3StorageService
4.10 Move + adapt SeedData
4.11 Create DependencyInjection.cs
4.12 Move existing migrations (or regenerate initial migration)
4.13 Verify: Infrastructure builds
```

### Phase 5: API Layer

```
5.1  Create Program.cs with DI wiring (AddApplication + AddInfrastructure)
5.2  Move + adapt all existing endpoint files (8 files)
     - Rewrite each to dispatch MediatR commands/queries instead of calling services
5.3  Create new admin endpoints (Approvals, Moderation, AuditLogs, Dashboard)
5.4  Create ExceptionHandlingMiddleware
5.5  Configure CORS for Admin WASM origin
5.6  Verify: API builds and starts
5.7  Verify: existing API routes return same responses (regression check)
```

### Phase 6: Admin WASM Layer

```
6.1  Create Blazor WASM project with HttpClient configuration
6.2  Create ApiClient base service (auth token handling)
6.3  Create typed API service classes
6.4  Build pages (separate document — out of scope here)
```

### Phase 7: Integration

```
7.1  Full solution build: dotnet build RealEstate.sln
7.2  Run test projects (compilation check)
7.3  API smoke test: hit existing endpoints, verify responses
7.4  Admin WASM: loads and can authenticate
```

---

## 5. Key Architecture Changes (Before vs After)

### 5.1 Request Flow

**Before (RealinServer monolith)**:
```
HTTP Request → Endpoint → Service → DbContext → Response
```

**After (Clean Architecture)**:
```
HTTP Request → Endpoint → MediatR.Send(Command) → Handler → Repository → DbContext → Response
```

**Admin WASM flow**:
```
User Click → Blazor Page → HttpClient → API Endpoint → MediatR → Handler → Repository → DB
```

### 5.2 Error Handling

**Before**: `(bool Success, T? Data, string? Error)` tuples returned from services.

**After**: Domain exceptions (`NotFoundException`, `BusinessRuleException`, `ForbiddenException`) thrown from handlers, caught by `ExceptionHandlingMiddleware` in the API and mapped to HTTP status codes (404, 422, 403).

### 5.3 Validation

**Before**: Manual if-checks inside service methods.

**After**: `FluentValidation` validators per command/query. `ValidationBehavior` MediatR pipeline runs them automatically before the handler executes.

### 5.4 Audit Logging

**Before**: None.

**After**: Commands that implement `IAuditableCommand` are automatically logged by `AuditLogBehavior` pipeline. Logs action, entity type/id, performing user, and timestamp to `audit_logs` table.

### 5.5 Soft Delete

**Before**: Hard delete via `DbContext.Remove()`.

**After**: Global query filter `entity.HasQueryFilter(e => !e.IsDeleted)` on all soft-deletable entities. Delete operations set `IsDeleted = true` + `DeletedAt` + `DeletedBy`.

---

## 6. File Count Summary

| Layer | Moved from RealinServer | New Files | Total |
|-------|------------------------|-----------|-------|
| Domain | 21 (entities + enums) | 10 (value objects, exceptions, AuditLog, new enums) | ~31 |
| Application | 12 (DTOs adapted) | ~65 (interfaces, features, common, behaviors) | ~77 |
| Infrastructure | 12 (DbContext, services, auth, messaging, seed) | ~28 (configs, repositories, DI, S3, CurrentUser) | ~40 |
| Api | 9 (endpoints + Program.cs) | ~6 (new admin endpoints, middleware) | ~15 |
| Admin | 0 | ~20 (services, pages shell, auth) | ~20 |
| **Total** | **54** | **~129** | **~183** |

---

## 7. Migration Safety

### Zero-Downtime Strategy

This migration does NOT require taking down the existing API. The approach:

1. **Build the new solution alongside the old one** — both exist simultaneously
2. **Verify the new API returns identical responses** on all existing routes
3. **Switch over** by deploying the new `RealEstate.Api` in place of old `RealinApi`
4. **Delete the old `RealinServer/` directory** only after successful cutover

### Rollback Plan

If the new API has issues after deployment, roll back to the old `RealinApi` (which still works unchanged). The database changes are backward-compatible — old code ignores new columns.

---

## 8. What This Plan Does NOT Cover

- **Blazor WASM pages, components, and layouts** — separate document
- **Admin authentication flow design** — JWT token handling in WASM, login page, AuthStateProvider
- **Test implementations** — test project structure is ready, tests come later
- **CI/CD pipeline and Docker** — out of scope
- **WASM deployment strategy** (S3 + CloudFront vs. served by API) — to be decided
