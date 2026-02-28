# Real Estate Admin Panel -- Final Tech Stack Decision

## 1. Overview

This document defines the final technology stack chosen for the Real
Estate Platform, including:

-   Admin Panel (Web)
-   Mobile App Backend
-   OTP-based Authentication
-   Agent Approval Workflow
-   Property Management System

The decision prioritizes:

-   Single primary language
-   Developer productivity (single developer)
-   Maintainability
-   Scalability
-   Security
-   Clean architecture

------------------------------------------------------------------------

# 2. Core Decision Summary

The platform will use **C# and .NET as the primary technology stack**.

------------------------------------------------------------------------

# 3. Final Technology Stack

## Backend

-   Framework: ASP.NET Core
-   Architecture: Clean Architecture (Layered)
-   Authentication: OTP + JWT
-   Authorization: Role-based + Policy-based
-   ORM: Entity Framework Core
-   Database: PostgreSQL
-   Real-Time: SignalR
-   File Storage: AWS S3
-   SMS Provider: AWS SNS / MSG91
-   Deployment: AWS EC2 / ECS

## Admin Panel (Web)

-   Framework: Blazor Server
-   Language: C#
-   UI Rendering: Server-side (SignalR-based)
-   Authentication: Cookie-based
-   Authorization: ASP.NET Core Identity + Policies

## Mobile Backend Design

-   Login: OTP-based
-   Identity Key: Mobile Number (Unique)
-   Token System: JWT
-   Customer Role: Auto-created on first OTP login
-   Agent Role: Explicit registration + admin approval required
-   Admin Role: Created internally

------------------------------------------------------------------------

# 4. Database Decision

Database: PostgreSQL

Core Tables: - Users - Roles - Agents - Properties - PropertyImages -
OTPRequests - Approvals - Notifications - AuditLogs

------------------------------------------------------------------------

# 5. Storage Decision

File Storage: AWS S3

Used for: - Property images - Agent documents

Approach: - Pre-signed upload URLs - Secure bucket policy - Optional
CloudFront CDN

------------------------------------------------------------------------

# 6. Architecture Pattern

The system follows:

-   Clean Architecture
-   Layered separation
-   Dependency injection
-   Domain-driven design principles (lightweight)

Layers:

1.  Presentation Layer (Blazor)
2.  Application Layer (Services)
3.  Domain Layer (Entities)
4.  Infrastructure Layer (EF Core, S3, SMS, JWT)

------------------------------------------------------------------------

# 7. Final Summary

Primary Language: C# Backend Framework: ASP.NET Core Admin UI: Blazor
Server Database: PostgreSQL Authentication: OTP + JWT Real-Time: SignalR
Storage: AWS S3 Deployment: AWS

This is the officially agreed technical direction for the Real Estate
Platform.
