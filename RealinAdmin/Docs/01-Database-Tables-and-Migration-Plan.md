# Database Tables & Migration Plan

## Purpose

This document analyzes the existing RealinServer database schema (PostgreSQL, `realin` schema) and identifies what needs to change to support the Admin Panel requirements defined in `Plan.md`.

The Admin Panel and the API server will **share the same PostgreSQL database**. The Admin Panel's `RealEstate.Infrastructure` project will have its own `AppDbContext` pointing at the same `realin` schema, so any table changes must be coordinated.

---

## 1. Current Database Schema (As-Is)

### 1.1 Identity & Auth Tables

| Table | Columns | Purpose |
|-------|---------|---------|
| `roles` | id, role_type, name, description, is_active, created_at, updated_at | 5-tier role hierarchy (Guest 0 / User 1 / Agent 5 / Admin 10 / SuperAdmin 100) |
| `modules` | id, code, name, description, is_active, created_at, updated_at | Permission modules (USER_MGMT, PROPERTY_BROWSE, PROPERTY_MANAGE, BOOKMARKS, ANALYTICS, SETTINGS) |
| `role_permissions` | id, role_id, module_id, can_read, can_create, can_update, can_delete, can_manage, created_at, updated_at | Granular permission matrix. Unique on (role_id, module_id) |
| `users` | id, email, phone_number, name, provider, oauth_provider_id, role_id, is_active, created_at, updated_at | All users (customers + agents + admins). Check constraint: email OR phone required |
| `refresh_tokens` | id, token, user_id, expires_at, created_at, is_revoked, device_info | JWT refresh tokens |
| `otp_sessions` | id, email, phone_number, otp_code, delivery_method, created_at, expires_at, is_verified, attempt_count, user_id | OTP auth flow |

### 1.2 Property Domain Tables

| Table | Columns | Purpose |
|-------|---------|---------|
| `properties` | id, title, listing_type, listing_category, property_type, construction_status, rera_id, available_from, status, agent_id, project_id, address, locality, city, pin_code, landmark, latitude, longitude, floor_number, total_floors, facing, price, currency, monthly_rent, security_deposit, maintenance_charges, price_negotiable, carpet_area, builtup_area, super_builtup_area, bedrooms, bathrooms, balconies, furnishing_status, property_age, ownership_type, loan_available, video_url, image_url, amenities (jsonb), interior_features (jsonb), utilities (jsonb), is_published, is_featured, created_at, updated_at | Core property listings |
| `agents` | id, user_id, license_number, agency_name, experience_years, rating, created_at, updated_at | Agent profiles (1:1 with users) |
| `builders` | id, name, email, phone, established_year, registration_number, headquarters_address, website, active, created_at, created_by_agent_id | Builder companies |
| `projects` | id, name, rera_id, builder_id, address, locality, city, pin_code, landmark, latitude, longitude, construction_status, launch_date, possession_date, status, total_towers, total_units, created_at, created_by_agent_id | Builder projects |
| `media` | id, property_id, storage_bucket, storage_key, url, uploaded_at | Property images/videos in S3 |

### 1.3 Engagement Tables

| Table | Columns | Purpose |
|-------|---------|---------|
| `favorites` | id, user_id, property_id, created_at | User bookmarks. Unique on (user_id, property_id) |
| `leads` | id, user_id, property_id, assigned_agent_id, status, notes, source, created_at, updated_at, contacted_at, converted_at | Sales pipeline |
| `inquiries` | id, user_id, property_id, message, contact_phone, contact_email, preferred_contact_time, status, response, created_at, responded_at | Property questions |

### 1.4 Existing Indexes

- `users`: unique on email, phone_number, (provider + oauth_provider_id); index on role_id
- `roles`: unique on role_type; index on name
- `modules`: unique on code; index on name
- `role_permissions`: unique on (role_id, module_id)
- `properties`: indexes on project_id, agent_id, city, listing_category, property_type, status, is_published, is_featured
- `agents`: unique on user_id, license_number
- `projects`: indexes on builder_id, city, rera_id
- `builders`: indexes on name, email
- `leads`: indexes on user_id, property_id, assigned_agent_id, status, created_at
- `inquiries`: indexes on user_id, property_id, status, created_at
- `favorites`: unique on (user_id, property_id)

### 1.5 Existing Seed Data

- **5 Roles**: Guest, User, Agent, Admin, SuperAdmin (deterministic GUIDs)
- **6 Modules**: USER_MGMT, PROPERTY_BROWSE, PROPERTY_MANAGE, BOOKMARKS, ANALYTICS, SETTINGS (deterministic GUIDs)
- **20+ RolePermissions**: Seeded at runtime via `SeedPermissions.SeedDefaultPermissionsAsync()`

---

## 2. Gap Analysis: What the Admin Panel Needs

Comparing the current schema against `Plan.md` requirements:

### 2.1 MISSING: Audit Log Table

**Plan.md Section 4.9** requires: "All actions must be logged."

Fields needed: Action, Performed By, Entity Type, Entity ID, Timestamp, Details.

**Current state**: No `audit_logs` table exists.

**Verdict: NEW TABLE REQUIRED**

### 2.2 MISSING: Moderation & Blacklist Fields

**Plan.md Section 4.7** requires universal moderation fields on Users, Agents, Builders, and Projects:
- `is_blocked`
- `is_blacklisted`
- `blacklist_reason`
- `blocked_by` (admin user ID)
- `blocked_at`

**Current state**:
- `users` has `is_active` only
- `agents` has no status fields at all
- `builders` has `active` only
- `projects` has `status` (string: active/completed/on_hold)

**Verdict: ALTER TABLES REQUIRED** - Add moderation columns to users, agents, builders, projects

### 2.3 MISSING: Agent Verification Workflow Fields

**Plan.md Section 4.4** defines agent lifecycle: `Registered -> Pending Verification -> Approved -> Active (or Rejected)`

**Current state**: `agents` has no `status`, `verification_status`, or document tracking fields.

**Verdict: ALTER TABLE REQUIRED** - Add verification workflow columns to agents

### 2.4 MISSING: Property Approval Workflow Fields

**Plan.md Section 4.5** defines property lifecycle: `Draft -> Submitted -> Under Review -> Approved -> Published (or Rejected)`

**Current state**: `properties` has `status` (active/sold/rented/off_market) and `is_published` boolean. This tracks listing status, not approval workflow.

**Verdict: ALTER TABLE REQUIRED** - Add approval workflow columns to properties

### 2.5 MISSING: Soft Delete Support

**Plan.md Section 2** requires: "Soft delete (never hard delete in production)"

**Current state**: No `is_deleted` / `deleted_at` / `deleted_by` on any table. Cascade deletes are configured in EF Core.

**Verdict: ALTER TABLES REQUIRED** - Add soft delete columns to key entities

### 2.6 MISSING: Additional Permission Types

**Plan.md Section 4.2** lists permission types: View, Create, Edit, Delete, **Approve**, **Block**, **Export**

**Current state**: `role_permissions` has: can_read, can_create, can_update, can_delete, can_manage

`can_manage` could cover Approve/Block, but **Export** is not represented. The `can_manage` field is a reasonable catch-all, but if you want fine-grained control, new columns would be needed.

**Verdict: OPTIONAL** - `can_manage` can cover Approve/Block for now. Add `can_export` if you want explicit export permission control.

### 2.7 MISSING: Additional Admin Roles

**Plan.md Section 4.2** suggests roles beyond the current 5: Operations Manager, Agent Verification Team, Support Executive, Finance Team.

**Current state**: The roles table is flexible - new roles can be inserted. But `RoleType` enum only has 5 values (Guest=0, User=1, Agent=5, Admin=10, SuperAdmin=100).

**Verdict: NO SCHEMA CHANGE** - New roles can be created in the roles table. The `RoleType` enum may need additional values in C# code but the DB column stores integers, so no migration needed. The module-permission system already supports arbitrary roles.

### 2.8 MISSING: Additional Modules

**Plan.md** implies modules for: Blacklist, Payments, Reports.

**Current state**: 6 modules seeded. The modules table is flexible.

**Verdict: DATA-ONLY CHANGE** - Insert new module rows. No schema change needed.

### 2.9 EXISTING: Tables That Need No Changes

These tables are fully adequate as-is:
- `roles` - Flexible enough
- `modules` - Flexible enough
- `role_permissions` - Flexible enough (with `can_manage` covering approve/block)
- `refresh_tokens` - Auth-only, no admin panel impact
- `otp_sessions` - Auth-only, no admin panel impact
- `media` - Adequate for property images
- `favorites` - Adequate
- `leads` - Adequate (already has status workflow)
- `inquiries` - Adequate (already has status workflow)

---

## 3. Proposed Schema Changes

### 3.1 NEW TABLE: `audit_logs`

```sql
CREATE TABLE realin.audit_logs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    action          VARCHAR(100) NOT NULL,          -- 'user.blocked', 'property.approved', 'agent.rejected'
    entity_type     VARCHAR(50) NOT NULL,           -- 'User', 'Property', 'Agent', 'Builder', 'Project'
    entity_id       UUID NOT NULL,                  -- ID of the affected entity
    performed_by    UUID NOT NULL REFERENCES realin.users(id),
    details         JSONB DEFAULT '{}',             -- Arbitrary context: old values, new values, reason
    ip_address      VARCHAR(45),                    -- IPv4/IPv6
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX ix_audit_logs_entity       ON realin.audit_logs (entity_type, entity_id);
CREATE INDEX ix_audit_logs_performed_by ON realin.audit_logs (performed_by);
CREATE INDEX ix_audit_logs_action       ON realin.audit_logs (action);
CREATE INDEX ix_audit_logs_created_at   ON realin.audit_logs (created_at);
```

### 3.2 ALTER TABLE: `users` - Add Moderation Fields

```sql
ALTER TABLE realin.users
    ADD COLUMN is_blocked        BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN is_blacklisted    BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN blacklist_reason  VARCHAR(500),
    ADD COLUMN blocked_by        UUID REFERENCES realin.users(id),
    ADD COLUMN blocked_at        TIMESTAMPTZ,
    ADD COLUMN is_deleted        BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN deleted_at        TIMESTAMPTZ,
    ADD COLUMN deleted_by        UUID REFERENCES realin.users(id);
```

### 3.3 ALTER TABLE: `agents` - Add Verification & Moderation Fields

```sql
ALTER TABLE realin.agents
    ADD COLUMN status               VARCHAR(50) NOT NULL DEFAULT 'pending',  -- pending, approved, rejected, suspended, blacklisted
    ADD COLUMN verification_notes   VARCHAR(1000),                           -- Admin notes on approval/rejection
    ADD COLUMN verified_by          UUID REFERENCES realin.users(id),
    ADD COLUMN verified_at          TIMESTAMPTZ,
    ADD COLUMN id_proof_url         VARCHAR(1000),                           -- Document uploads
    ADD COLUMN company_details      JSONB DEFAULT '{}',                      -- Flexible company info
    ADD COLUMN is_blocked           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN is_blacklisted       BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN blacklist_reason     VARCHAR(500),
    ADD COLUMN blocked_by           UUID REFERENCES realin.users(id),
    ADD COLUMN blocked_at           TIMESTAMPTZ,
    ADD COLUMN is_deleted           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN deleted_at           TIMESTAMPTZ;

CREATE INDEX ix_agents_status ON realin.agents (status);
```

### 3.4 ALTER TABLE: `properties` - Add Approval Workflow & Soft Delete

```sql
ALTER TABLE realin.properties
    ADD COLUMN approval_status      VARCHAR(50) NOT NULL DEFAULT 'draft',    -- draft, submitted, under_review, approved, rejected
    ADD COLUMN rejection_reason     VARCHAR(1000),
    ADD COLUMN reviewed_by          UUID REFERENCES realin.users(id),
    ADD COLUMN reviewed_at          TIMESTAMPTZ,
    ADD COLUMN submitted_at         TIMESTAMPTZ,
    ADD COLUMN is_flagged           BOOLEAN NOT NULL DEFAULT FALSE,          -- Flagged for inappropriate content
    ADD COLUMN flag_reason          VARCHAR(500),
    ADD COLUMN is_deleted           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN deleted_at           TIMESTAMPTZ,
    ADD COLUMN deleted_by           UUID REFERENCES realin.users(id);

CREATE INDEX ix_properties_approval_status ON realin.properties (approval_status);
```

### 3.5 ALTER TABLE: `builders` - Add Moderation & Soft Delete

```sql
ALTER TABLE realin.builders
    ADD COLUMN is_blocked           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN is_blacklisted       BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN blacklist_reason     VARCHAR(500),
    ADD COLUMN blocked_by           UUID REFERENCES realin.users(id),
    ADD COLUMN blocked_at           TIMESTAMPTZ,
    ADD COLUMN is_deleted           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN deleted_at           TIMESTAMPTZ,
    ADD COLUMN updated_at           TIMESTAMPTZ DEFAULT NOW();                -- Currently missing
```

### 3.6 ALTER TABLE: `projects` - Add Moderation & Soft Delete

```sql
ALTER TABLE realin.projects
    ADD COLUMN is_blocked           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN is_blacklisted       BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN blacklist_reason     VARCHAR(500),
    ADD COLUMN blocked_by           UUID REFERENCES realin.users(id),
    ADD COLUMN blocked_at           TIMESTAMPTZ,
    ADD COLUMN is_deleted           BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN deleted_at           TIMESTAMPTZ,
    ADD COLUMN updated_at           TIMESTAMPTZ DEFAULT NOW();                -- Currently missing
```

### 3.7 OPTIONAL: Add `can_export` to `role_permissions`

```sql
ALTER TABLE realin.role_permissions
    ADD COLUMN can_export BOOLEAN NOT NULL DEFAULT FALSE;
```

### 3.8 SEED DATA: New Modules

```sql
INSERT INTO realin.modules (id, code, name, description, is_active, created_at, updated_at) VALUES
    ('10000000-0000-0000-0000-000000000007', 'BLACKLIST',  'Blacklist Management', 'Manage blacklisted entities', true, NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000008', 'AUDIT_LOGS', 'Audit Logs',          'View system audit logs',       true, NOW(), NOW()),
    ('10000000-0000-0000-0000-000000000009', 'REPORTS',    'Reports & Export',     'Generate and export reports',  true, NOW(), NOW()),
    ('10000000-0000-0000-0000-00000000000a', 'APPROVALS',  'Approvals',           'Manage approval workflows',    true, NOW(), NOW());
```

---

## 4. Summary of Changes

| Change Type | Table | What |
|-------------|-------|------|
| **NEW TABLE** | `audit_logs` | Full audit trail for admin actions |
| **ADD COLUMNS** | `users` | is_blocked, is_blacklisted, blacklist_reason, blocked_by, blocked_at, soft delete |
| **ADD COLUMNS** | `agents` | status, verification fields, moderation fields, soft delete |
| **ADD COLUMNS** | `properties` | approval_status, rejection_reason, review fields, flagging, soft delete |
| **ADD COLUMNS** | `builders` | moderation fields, soft delete, updated_at |
| **ADD COLUMNS** | `projects` | moderation fields, soft delete, updated_at |
| **ADD COLUMN** | `role_permissions` | can_export (optional) |
| **SEED DATA** | `modules` | 4 new modules: BLACKLIST, AUDIT_LOGS, REPORTS, APPROVALS |

---

## 5. Migration Strategy

### 5.1 Where Migrations Live

Since both RealinServer and RealinAdmin share the same database, migrations should be managed from **one place only** to avoid conflicts.

**Recommendation**: Keep EF Core migrations in `RealinServer/RealinApi` for now (it owns the current migration history). The Admin Panel's `RealEstate.Infrastructure` will share the same schema but will NOT generate its own migrations until you decide to move migration ownership.

**Alternative (future)**: Extract a shared `RealEstate.Database` project that both solutions reference for migrations. This is cleaner long-term but adds complexity now.

### 5.2 Migration Execution Order

All changes are additive (ADD COLUMN, CREATE TABLE) so they are safe against existing data.

```
Migration 4: AddAuditLogsTable
Migration 5: AddModerationFieldsToUsers
Migration 6: AddVerificationAndModerationToAgents
Migration 7: AddApprovalWorkflowToProperties
Migration 8: AddModerationToBuilders
Migration 9: AddModerationToProjects
Migration 10: AddExportPermission (optional)
Migration 11: SeedNewModules
```

Or combine into fewer migrations:

```
Migration 4: AddAdminPanelSchemaChanges     -- All ALTER TABLEs + CREATE audit_logs
Migration 5: SeedAdminPanelModules          -- New module seed data
```

### 5.3 Steps to Execute

1. **Update entities** in RealinServer's `Data/Entities/` to add new properties
2. **Update `AppDbContext`** fluent configuration for new columns
3. Run `dotnet ef migrations add AddAdminPanelSchemaChanges`
4. Review generated migration
5. Run `dotnet ef database update` against dev database
6. Verify with `\d realin.audit_logs` etc.

### 5.4 Backward Compatibility

All changes are backward-compatible:
- New columns have default values, so existing INSERT statements still work
- No columns are removed or renamed
- No type changes on existing columns
- Existing RealinServer code continues to work without modification (it just won't use the new columns)

### 5.5 Data Migration

No data transformation needed. All new columns default to `FALSE`, `NULL`, or `'draft'`/`'pending'` which are safe defaults.

One consideration: existing `agents` rows will get `status = 'pending'` by default. You may want a one-time data fix:

```sql
-- Mark all existing agents as approved (they were created before verification existed)
UPDATE realin.agents SET status = 'approved', verified_at = created_at WHERE status = 'pending';
```

Similarly for existing properties:

```sql
-- Mark all existing published properties as approved
UPDATE realin.properties SET approval_status = 'approved', reviewed_at = created_at WHERE is_published = true;

-- Mark all existing unpublished properties as draft
UPDATE realin.properties SET approval_status = 'draft' WHERE is_published = false;
```

---

## 6. Impact on RealinServer API

The RealinServer API code does **not** need to change for the schema additions - it will simply ignore the new columns. However, you should eventually update it to:

1. **Respect soft deletes**: Add `.Where(x => !x.IsDeleted)` to queries (or use EF Core global query filters)
2. **Respect blocked/blacklisted status**: Check these flags in auth and listing queries
3. **Write audit logs**: When admin endpoints modify users/roles/properties

These API changes are **not blocking** for the Admin Panel development and can be done incrementally.

---

## 7. Decision Points for You

| # | Question | Options |
|---|----------|---------|
| 1 | Should `can_export` be a separate permission column? | A) Yes, add it. B) No, use `can_manage` or `can_read` to control exports. |
| 2 | Should migrations be a single large migration or multiple small ones? | A) Single (simpler). B) Multiple (easier to review/revert). |
| 3 | Should we create a shared `RealEstate.Database` project for migrations? | A) Yes (cleaner long-term). B) No, keep migrations in RealinServer for now. |
| 4 | Agent `company_details` as JSONB vs. separate columns? | A) JSONB (flexible). B) Separate columns (stricter, better querying). |
