# Real Estate Admin Panel -- Navigation Map Diagram

## 1. Top-Level Navigation Map

    Dashboard
    │
    ├── Users
    │     ├── All Users
    │     ├── Create User
    │     └── User Details
    │
    ├── Roles & Permissions
    │     ├── Roles List
    │     ├── Create Role
    │     └── Permission Matrix
    │
    ├── Agents
    │     ├── All Agents
    │     ├── Pending Approval
    │     ├── Suspended
    │     └── Agent Details
    │
    ├── Properties
    │     ├── All Properties
    │     ├── Pending Approval
    │     ├── Rejected
    │     ├── Archived
    │     └── Property Details
    │
    ├── Approvals (Operational Queue)
    │     ├── Agent Approvals
    │     ├── Property Approvals
    │     └── Change Requests
    │
    ├── Analytics
    │     ├── Business Overview
    │     ├── Agent Performance
    │     ├── Property Insights
    │     └── Search Analytics
    │
    ├── Audit Logs
    │
    └── Settings
          ├── System Settings
          ├── Notification Rules
          └── Feature Flags (Future)

------------------------------------------------------------------------

## 2. Dashboard Drill-Down Flow

Example Flow:

    Dashboard
       ↓ (Click Pending Properties)
    Properties → Filtered by Status = Pending
       ↓
    Property Detail
       ↓
    Approve
       ↓
    Return to Filtered List

Purpose: - Quick operational navigation - Direct access to problem
areas - Reduced click depth

------------------------------------------------------------------------

## 3. Users Navigation Flow

    Users (Table View)
       ↓
    User Detail Page
       ↓
    Edit User (Modal or Side Panel)
       ↓
    Assign Role
       ↓
    Save
       ↓
    Return to Detail

Design Notes: - Server-side pagination - Modal-based editing - Role
filtering

------------------------------------------------------------------------

## 4. Agent Workflow Navigation

    Approvals → Agent Approvals
       ↓
    Select Agent
       ↓
    Right-side Detail Panel
       ↓
    Approve / Reject
       ↓
    Auto move to next queue item

Design Type: - Queue-driven - High-efficiency review screen - Minimal
navigation switching

------------------------------------------------------------------------

## 5. Property Navigation Flow

    Properties (Table)
       ↓
    Property Detail
       ↓
    Tabs:
        - Overview
        - Media
        - Activity
        - Approval History
       ↓
    Action (Approve / Reject / Archive)

Design Notes: - Color-coded status - S3-backed image gallery - Workflow
timeline view

------------------------------------------------------------------------

## 6. Analytics Navigation Flow

    Analytics
       ├── Business Overview
       ├── Agent Performance
       ├── Property Insights
       └── Search Analytics

Behavior: - Global filters (Date Range, City, Agent) - Click chart
elements → Navigate to filtered property list - Elasticsearch-powered
aggregations

------------------------------------------------------------------------

## 7. Route Structure (Blazor Server)

    /
    /users
    /users/{id}
    /roles
    /roles/{id}
    /agents
    /agents/{id}
    /properties
    /properties/{id}
    /approvals/agents
    /approvals/properties
    /analytics/overview
    /analytics/agents
    /analytics/properties
    /audit
    /settings

------------------------------------------------------------------------

## 8. Role-Based Navigation Visibility

  Role         Visible Modules
  ------------ ----------------------------
  SuperAdmin   All Modules
  Admin        All except System Settings
  Reviewer     Approvals, Properties
  Support      Users (View), Agents
  Agent        Own Properties Only

Sidebar rendering should be dynamic based on JWT claims.

------------------------------------------------------------------------

## 9. Cross-Module Navigation Links

Examples:

-   Agent Detail → Click Property Count → Navigate to Properties
    filtered by AgentId
-   Property Detail → Click Agent Name → Navigate to Agent Detail
-   Analytics Chart → Click Bar → Navigate to Filtered Property List

------------------------------------------------------------------------

## 10. Breadcrumb Strategy

Example:

    Dashboard > Properties > Property #12345

Purpose: - Prevent navigation confusion - Maintain hierarchical
clarity - Improve operational usability

------------------------------------------------------------------------

# Navigation Model Summary

Hybrid Enterprise Model:

-   Sidebar-based primary navigation
-   Drill-down detail pages
-   Modal-based edits
-   Queue-driven operational screens
-   Analytics-driven data exploration
-   Role-aware dynamic visibility

This structure ensures scalability, clarity, and operational efficiency
for the real estate admin platform.
