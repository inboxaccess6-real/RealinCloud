Admin Panel Plan – Real Estate Application

1. Overview

This document outlines the functional and structural plan for building the Admin Panel (Back Office System) for the Real Estate ecosystem.

The Admin Panel will control:
	•	Mobile App Users (Customers)
	•	Real Estate Agents
	•	Builders
	•	Projects
	•	Properties
	•	Roles & Permissions
	•	Moderation & Blacklisting
	•	Reporting & Audit Logs

This is not just an admin page — it is a full operational control system.

⸻

2. Core Design Principles
	•	Role-Based Access Control (RBAC)
	•	Entity-based modular architecture
	•	Status-driven workflows
	•	Approval queues
	•	Soft delete (never hard delete in production)
	•	Audit logging for all actions
	•	Advanced filtering & bulk actions
	•	Export capabilities (CSV/Excel)

⸻

3. High-Level Conceptual Architecture

Three primary layers:
	1.	Customer App (Mobile Listing App)
	2.	Agent Portal
	3.	Admin Panel (Back Office)

The Admin Panel governs and moderates all entities created through the other two systems.

⸻

4. Core Modules

4.1 Authentication & Access Control Module

Purpose: Secure internal admin system.

Features:
	•	Admin login
	•	Role-based access
	•	Session management
	•	Password policy
	•	Optional 2FA

⸻

4.2 Role & Permission Management Module

Entities:
	•	Role
	•	Module
	•	Permission

Example Roles:
	•	Super Admin
	•	Admin
	•	Operations Manager
	•	Agent Verification Team
	•	Support Executive
	•	Finance Team

Example Modules:
	•	Users
	•	Agents
	•	Builders
	•	Projects
	•	Properties
	•	Payments
	•	Reports
	•	Blacklist
	•	Settings

Permission Types:
	•	View
	•	Create
	•	Edit
	•	Delete
	•	Approve
	•	Block
	•	Export

Permission Matrix Example:

Role	Module	Permission
Ops	Agents	Approve
Support	Users	Block
Admin	All	Full


⸻

4.3 User Management Module (Customers)

Fields:
	•	Name
	•	Email
	•	Phone
	•	KYC status
	•	Registration source
	•	Device info
	•	Status

User Status:
	•	Active
	•	Blocked
	•	Suspended
	•	Blacklisted

Admin Capabilities:
	•	View details
	•	Block / Unblock
	•	Add to blacklist
	•	View activity logs
	•	Reset password
	•	View associated favorites/bookings

⸻

4.4 Agent Management Module

Agent Lifecycle

Registered → Pending Verification → Approved → Active
↘ Rejected

Agent Registration Requirements:
	•	ID proof
	•	RERA number (if applicable)
	•	Company details

Agent Status:
	•	Pending
	•	Approved
	•	Rejected
	•	Suspended
	•	Blacklisted

Admin Actions:
	•	Verify documents
	•	Approve / Reject with remarks
	•	Suspend / Blacklist

⸻

4.5 Property Management Module

Property Lifecycle

Draft → Submitted → Under Review → Approved → Published
↘ Rejected

Admin Capabilities:
	•	Approve property
	•	Reject with reason
	•	Edit property if necessary
	•	Remove listing
	•	Flag inappropriate content

Property Fields:
	•	Title
	•	Type
	•	Price
	•	Location
	•	Builder
	•	Images
	•	Documents
	•	Status
	•	Created by (Agent ID)

⸻

4.6 Builder & Project Management

Builders:
	•	Can have multiple projects
	•	Can have associated agents

Builder Status:
	•	Active
	•	Suspended
	•	Blacklisted

Admin Capabilities:
	•	Block builder
	•	View associated projects
	•	Monitor complaints

⸻

4.7 Moderation & Blacklist System

Instead of deleting entities, use universal status fields:
	•	isActive
	•	isBlocked
	•	isBlacklisted
	•	blacklistReason
	•	blockedBy
	•	blockedAt

Blacklisted entities:
	•	Cannot login
	•	Cannot post properties
	•	Cannot appear in search

Applicable To:
	•	Users
	•	Agents
	•	Builders
	•	Projects

⸻

4.8 Dashboard Module

Dashboard Metrics:
	•	Total Users
	•	Total Agents
	•	Pending Agent Approvals
	•	Pending Property Approvals
	•	Total Projects
	•	Blacklisted Entities
	•	Revenue (if applicable)
	•	Activity trends (daily/weekly/monthly)

⸻

4.9 Audit Log Module

All actions must be logged.

Audit Log Fields:
| Action | Performed By | Entity | Timestamp |

Examples:
	•	Blocked User
	•	Approved Agent
	•	Rejected Property

⸻

4.10 Reporting Module

Reports:
	•	Daily registrations
	•	Agent growth
	•	Property growth
	•	Conversion rates
	•	Region-wise listings

⸻

5. Workflow Design

5.1 Agent Onboarding Flow
	1.	Agent registers
	2.	System marks as Pending
	3.	Admin notified
	4.	Admin verifies documents
	5.	Approve / Reject
	6.	Agent notified

5.2 Property Upload Flow
	1.	Agent uploads property
	2.	Status = Submitted
	3.	Goes to review queue
	4.	Admin approves
	5.	Property goes live

⸻

6. Navigation Structure

Suggested Admin Navigation:
	•	Dashboard
	•	Users
	•	Agents
	•	Builders
	•	Projects
	•	Properties
	•	Approvals
	•	Blacklist
	•	Roles & Permissions
	•	Reports
	•	Audit Logs
	•	Settings

⸻

7. Advanced Features (Optional)
	•	AI-based image moderation
	•	Duplicate property detection
	•	Fraud scoring
	•	Geo analytics
	•	Revenue tracking
	•	Commission tracking
	•	Complaint management system

⸻

8. Scalability Considerations
	•	Separate admin queries from customer queries
	•	Use read replicas for reporting
	•	Use indexed search for filtering
	•	Precompute dashboard metrics where possible
	•	Use soft delete and status flags instead of hard delete

⸻

9. Structural Recommendation

Do NOT mix:
	•	Customer users
	•	Agents
	•	Admin users

Keep them separate entities with clearly defined relationships.

⸻

10. Next Steps

Possible next documentation expansions:
	•	Database schema design
	•	Detailed permission schema
	•	API design structure
	•	UI wireframe structure
	•	Multi-tenant architecture plan
	•	Detailed moderation engine design

⸻

End of Admin Panel Plan Document