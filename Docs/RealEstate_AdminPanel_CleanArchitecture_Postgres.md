# Real Estate Admin Panel -- Clean Architecture Design (Updated)

## Technology Stack (Current)

-   Backend: ASP.NET Core
-   UI: Blazor Server
-   Database: PostgreSQL
-   Search: (Future) Elasticsearch
-   File Storage: S3 (for property images)

------------------------------------------------------------------------

# 1. Architecture Style

We will use **Clean Architecture (Domain-Centered Design)**.

This ensures:

-   Separation of concerns
-   Long-term scalability
-   Replaceable infrastructure (e.g., add Elasticsearch later)
-   Testable business logic
-   Thin UI layer

------------------------------------------------------------------------

# 2. Solution Structure

    RealEstate.sln
    │
    ├── RealEstate.Domain
    ├── RealEstate.Application
    ├── RealEstate.Infrastructure
    ├── RealEstate.Web (Blazor Server)
    └── RealEstate.Contracts (Optional)

------------------------------------------------------------------------

# 3. Domain Layer (Core Business)

Project: `RealEstate.Domain`

Contains:

-   Entities
-   Enums
-   Value Objects
-   Domain Rules
-   Business Invariants

### Structure

    Domain/
    │
    ├── Entities/
    │     ├── User.cs
    │     ├── Role.cs
    │     ├── Agent.cs
    │     ├── Property.cs
    │
    ├── Enums/
    │     ├── PropertyStatus.cs
    │     ├── AgentStatus.cs
    │
    ├── ValueObjects/
    │     ├── Email.cs
    │     ├── Money.cs
    │
    └── Exceptions/

### Important Rule

Domain must NOT depend on:

-   PostgreSQL
-   Entity Framework
-   Blazor
-   ASP.NET
-   Elasticsearch

It contains only business logic.

------------------------------------------------------------------------

# 4. Application Layer

Project: `RealEstate.Application`

Responsible for:

-   Use cases
-   Commands & Queries
-   DTOs
-   Interfaces
-   Validation
-   Business orchestration

### Structure

    Application/
    │
    ├── Interfaces/
    │     ├── IPropertyRepository.cs
    │     ├── IAgentRepository.cs
    │     ├── IUserRepository.cs
    │     ├── IRoleRepository.cs
    │
    ├── Features/
    │     ├── Properties/
    │     │      ├── CreatePropertyCommand.cs
    │     │      ├── ApprovePropertyCommand.cs
    │     │      ├── GetPropertiesQuery.cs
    │     │
    │     ├── Agents/
    │     ├── Users/
    │     ├── Roles/
    │
    ├── DTOs/
    │
    └── Services/

------------------------------------------------------------------------

# 5. Infrastructure Layer

Project: `RealEstate.Infrastructure`

Responsible for:

-   PostgreSQL persistence (via EF Core)
-   Identity implementation
-   S3 storage
-   External integrations
-   (Future) Elasticsearch implementation

### Structure

    Infrastructure/
    │
    ├── Persistence/
    │     ├── AppDbContext.cs
    │     ├── Configurations/
    │     ├── Repositories/
    │     │      ├── PropertyRepository.cs
    │     │      ├── AgentRepository.cs
    │
    ├── Identity/
    │
    ├── Storage/
    │     ├── S3StorageService.cs
    │
    ├── Search/ (Future)
    │     ├── ElasticsearchService.cs
    │
    └── DependencyInjection.cs

### PostgreSQL Strategy

-   Use EF Core
-   Proper indexing on:
    -   Property.City
    -   Property.Status
    -   Agent.Status
    -   CreatedDate
-   Use pagination everywhere
-   Avoid loading large datasets fully

------------------------------------------------------------------------

# 6. Web Layer (Blazor Server)

Project: `RealEstate.Web`

Contains:

-   Razor Pages
-   Layouts
-   UI Components
-   Authorization policies
-   ViewModels

### Structure

    Web/
    │
    ├── Pages/
    │     ├── Dashboard.razor
    │     ├── Users/
    │     ├── Agents/
    │     ├── Properties/
    │
    ├── Components/
    │
    ├── Layouts/
    │
    ├── Shared/
    │
    └── Program.cs

Blazor Server Responsibilities:

-   Render UI
-   Call Application layer
-   Handle authorization
-   Manage UI state

------------------------------------------------------------------------

# 7. Dependency Flow

    Blazor Web
         ↓
    Application Layer
         ↓
    Domain Layer

    Infrastructure plugs into Application via interfaces

Rules:

-   Domain depends on nothing
-   Application depends only on Domain
-   Infrastructure depends on Application
-   Web depends on Application

------------------------------------------------------------------------

# 8. Future Elasticsearch Integration

When mobile app scales:

1.  Add ISearchService interface in Application
2.  Implement ElasticsearchService in Infrastructure
3.  Keep PostgreSQL as primary data store
4.  Use Elasticsearch for:
    -   Aggregations
    -   Complex filtering
    -   Search optimization
    -   Analytics dashboards

This allows smooth migration without touching Domain.

------------------------------------------------------------------------

# 9. Authorization Strategy

-   ASP.NET Identity in Infrastructure
-   Role-based authorization
-   Policy-based permissions
-   Blazor uses \[Authorize\] attributes
-   Permission matrix handled in Application layer

------------------------------------------------------------------------

# 10. Why This Architecture Fits Current Phase

Since:

-   You are using PostgreSQL
-   Elasticsearch is future enhancement
-   This is an internal admin panel

This structure:

-   Keeps system simple today
-   Keeps room for scale tomorrow
-   Avoids over-engineering
-   Keeps domain logic clean

------------------------------------------------------------------------

# Architecture Summary

Clean Architecture with:

-   PostgreSQL as primary database
-   EF Core in Infrastructure
-   Blazor Server UI
-   Domain-driven structure
-   Elasticsearch prepared but optional

This design ensures enterprise-grade maintainability and future
scalability.
