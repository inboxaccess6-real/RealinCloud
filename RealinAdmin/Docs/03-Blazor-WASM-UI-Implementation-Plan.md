# Blazor WASM Admin Panel - UI Implementation Plan

## 1. Overview

The Realin Admin Panel is a Blazor WebAssembly (WASM) application used by internal company staff (Admins, SuperAdmins) and external agents. It communicates with the RealEstate.Api backend via HTTP, authenticating with JWT tokens obtained through OTP-based login (mobile or email).

**Key Principles:**
- Mobile-first responsive design (agents will use mobile/tablet)
- Role-aware UI: sidebar, pages, and action buttons adapt to the logged-in user's permissions
- Theming-ready: all colors, spacing, and typography use CSS custom properties so Figma/Lovable designs can be applied as a theme layer later
- No component library lock-in initially - use semantic HTML + CSS custom properties, then optionally adopt MudBlazor or Radzen later

---

## 2. Authentication Flow

### 2.1 Login Screen (`/login`)

The login page is the entry point for all users. No registration flow - users are created by admins.

```
+--------------------------------------------------+
|                                                  |
|              [Realin Logo]                       |
|           Admin Portal                           |
|                                                  |
|   +------------------------------------------+  |
|   |  [ Email ]  [ Mobile ]   <- Tab toggle   |  |
|   +------------------------------------------+  |
|                                                  |
|   +------------------------------------------+  |
|   |  Email: [________________________]       |  |
|   |                                          |  |
|   |  [ Send OTP ]                            |  |
|   +------------------------------------------+  |
|                                                  |
|   --- After OTP sent ---                         |
|                                                  |
|   +------------------------------------------+  |
|   |  Enter OTP:                              |  |
|   |  [ _ ] [ _ ] [ _ ] [ _ ] [ _ ] [ _ ]    |  |
|   |                                          |  |
|   |  Resend in 00:45                         |  |
|   |                                          |  |
|   |  [ Verify & Login ]                      |  |
|   +------------------------------------------+  |
|                                                  |
+--------------------------------------------------+
```

**Flow:**
1. User selects **Email** or **Mobile** tab
2. Enters email address or mobile number (with country code picker for mobile)
3. Clicks **Send OTP** -> calls `POST /api/auth/otp/request`
4. OTP input appears (6 individual digit boxes with auto-focus advance)
5. Countdown timer (configurable, e.g. 60s) before "Resend OTP" becomes active
6. User enters OTP -> calls `POST /api/auth/otp/verify`
7. On success: receives `AuthResponse` with JWT tokens + `UserInfo`
8. Tokens stored in `localStorage` (access token + refresh token + expiry)
9. `UserInfo` (id, name, role, roleLevel) cached in memory state
10. Redirect to `/dashboard`

**Error States:**
- Invalid/expired OTP: shake animation on OTP boxes + inline error message
- Rate limited: disable Send OTP button, show "Too many attempts. Try again in X minutes."
- Account blocked/inactive: show "Your account has been deactivated. Contact administrator."

### 2.2 Auth State Management

```
Services/
  AuthStateProvider.cs      - Custom AuthenticationStateProvider
  AuthService.cs            - Login, logout, token refresh logic
  TokenService.cs           - localStorage read/write, token expiry check
```

**Token Refresh Strategy:**
- On every HTTP request, check if access token expires within 2 minutes
- If so, call `POST /api/auth/refresh` with the refresh token
- If refresh fails (401), redirect to `/login`
- `AuthorizingDelegatingHandler` wraps HttpClient to inject Bearer token automatically

### 2.3 Permission Loading

After login, immediately call a permissions endpoint (or decode from JWT claims if embedded):
- Cache the user's `Dictionary<string, HashSet<string>>` (module code -> permission set)
- This drives sidebar visibility and page-level authorization
- Refresh permissions on token refresh

---

## 3. Application Shell & Layout

### 3.1 Shell Structure

```
+------------------------------------------------------------------+
| [=] Logo        Search...          [Bell] [Avatar v]             |  <- Top Bar
+----------+-------------------------------------------------------+
|          |                                                       |
| Sidebar  |                  Main Content Area                    |
|          |                                                       |
| Dashboard|   Breadcrumb: Dashboard > Properties > #12345         |
| Users    |                                                       |
| Agents   |   +-----------------------------------------------+  |
| Props    |   |                                               |  |
| Builders |   |              Page Content                     |  |
| Projects |   |                                               |  |
| Approvals|   |                                               |  |
| Analytics|   |                                               |  |
| Audit    |   |                                               |  |
| Roles    |   |                                               |  |
| Settings |   +-----------------------------------------------+  |
|          |                                                       |
+----------+-------------------------------------------------------+
```

### 3.2 Top Bar

- **Hamburger menu** `[=]`: toggles sidebar collapse on desktop, opens overlay sidebar on mobile
- **Logo**: "Realin" text or logo image, links to `/dashboard`
- **Global Search**: searches across users, agents, properties (future phase)
- **Notification Bell**: pending approval count badge (pulls from dashboard metrics)
- **User Avatar Dropdown**: shows name, role badge, links to Profile, Logout

### 3.3 Sidebar Navigation

The sidebar renders dynamically based on the user's role permissions. Each menu item maps to a module code.

| Menu Item | Icon | Route | Module Code | Min Permission |
|-----------|------|-------|-------------|----------------|
| Dashboard | grid | `/dashboard` | - | Always visible |
| Users | users | `/users` | `USER_MGMT` | CanRead |
| Agents | shield | `/agents` | `AGENT_MGMT` | CanRead |
| Properties | building | `/properties` | `PROPERTY_MGMT` | CanRead |
| Builders | hard-hat | `/builders` | `PROPERTY_MGMT` | CanRead |
| Projects | layers | `/projects` | `PROPERTY_MGMT` | CanRead |
| Approvals | check-circle | `/approvals` | `APPROVALS` | CanRead |
| Analytics | bar-chart | `/analytics` | `REPORTS` | CanRead |
| Audit Logs | file-text | `/audit-logs` | `AUDIT_LOGS` | CanRead |
| Roles & Permissions | key | `/roles` | `ROLE_MGMT` | CanRead |
| Settings | settings | `/settings` | `SETTINGS` | CanRead |

**Sidebar Behavior:**
- Desktop (>1024px): persistent, collapsible to icon-only mode (56px width)
- Tablet (768-1024px): overlay mode, triggered by hamburger
- Mobile (<768px): full-screen overlay with backdrop
- Active route highlighted with accent color + left border indicator
- Expandable sub-items for Approvals (Agent / Property) and Analytics (Overview / Agent / Property)
- Badge counts on Approvals (pending count from dashboard metrics)

### 3.4 Breadcrumbs

Auto-generated from route segments:
- `Dashboard > Users > John Doe`
- `Dashboard > Properties > Pending Approval`
- Clickable segments for navigation back up the hierarchy

---

## 4. Role-Based UI Visibility

### 4.1 Role Hierarchy & Default Access

| Role | Level | Typical Access |
|------|-------|---------------|
| SuperAdmin | 100 | Full access to everything |
| Admin | 10 | All modules except system settings internals |
| Agent | 5 | Own properties, own profile, builders/projects they created |
| User | 1 | Browse properties, favorites, own profile |
| Guest | 0 | Public property browsing only (unlikely to use admin panel) |

### 4.2 Permission-Driven UI Rules

Permissions are checked at **three levels**:

1. **Sidebar visibility**: hide menu items the user cannot read
2. **Page-level guard**: if user navigates directly to a URL they lack permission for, show "Access Denied" page
3. **Action-level**: hide/disable buttons based on granular permissions

| Action | Required Permission |
|--------|-------------------|
| View list/detail | `CanRead` on module |
| Create new record | `CanCreate` on module |
| Edit existing record | `CanUpdate` on module |
| Delete record | `CanDelete` on module |
| Approve/Reject | `CanManage` on module |
| Export to CSV | `CanExport` on module |

### 4.3 Agent-Specific View

When an Agent logs in:
- Dashboard shows only their own metrics (my properties, my leads, my status)
- Properties list is pre-filtered to `AgentId = currentUser`
- Cannot see other agents' data
- Builders/Projects they created are visible
- No access to Users, Roles, Audit Logs, Settings

### 4.4 Implementation: `PermissionGuard` Component

```razor
<PermissionGuard Module="PROPERTY_MGMT" Permission="CanCreate">
    <button @onclick="CreateProperty">New Property</button>
</PermissionGuard>
```

If the user lacks the permission, the child content is not rendered.

---

## 5. Page Designs

### 5.1 Dashboard (`/dashboard`)

The landing page after login. Shows operational metrics and quick-action cards.

```
+------------------------------------------------------------------+
| Good morning, {Name}                           Role: Admin       |
+------------------------------------------------------------------+
|                                                                  |
| +------------+ +------------+ +------------+ +------------+     |
| | Total Users| | Total Props| | Tot Agents | | Builders   |     |
| |    1,234   | |    5,678   | |     234    | |    89      |     |
| +------------+ +------------+ +------------+ +------------+     |
|                                                                  |
| +-----------------------------+ +----------------------------+  |
| | Pending Actions             | | Quick Stats               |  |
| |                             | |                            |  |
| | [!] 12 Agent Verifications  | | Projects: 156             |  |
| |     -> Click to review      | | Flagged Props: 3          |  |
| |                             | |                            |  |
| | [!] 45 Property Approvals   | |                            |  |
| |     -> Click to review      | |                            |  |
| +-----------------------------+ +----------------------------+  |
|                                                                  |
| +-------------------------------------------------------------+ |
| | Recent Activity (Audit Log Preview)                         | |
| |                                                             | |
| | Admin updated Property #123          2 min ago              | |
| | Agent submitted Property #456       15 min ago              | |
| | Admin approved Agent #789            1 hour ago             | |
| +-------------------------------------------------------------+ |
+------------------------------------------------------------------+
```

**API Calls:** `GET /api/admin/dashboard/metrics`, `GET /api/admin/audit-logs?pageSize=5`

**Metric Cards:**
- Clickable: "Pending Agent Verifications" -> `/approvals/agents`
- Clickable: "Pending Property Approvals" -> `/approvals/properties`
- Clickable: "Flagged Properties" -> `/properties?flagged=true`

**Agent Dashboard Variant:**
- Show: My Properties (count), My Pending Approvals, My Builders, My Projects
- Hide: Total Users, Total Agents, system-wide metrics

---

### 5.2 Users (`/users`, `/users/{id}`)

**List View** - Data table with server-side pagination:

```
+------------------------------------------------------------------+
| Users                                    [ + Create User ]       |
+------------------------------------------------------------------+
| Search: [_______________]  Status: [All v]  Role: [All v]       |
+------------------------------------------------------------------+
| Name          | Email           | Phone    | Role  | Status | Act |
|---------------|-----------------|----------|-------|--------|-----|
| John Doe      | john@ex.com     | +91...   | Admin | Active | ... |
| Jane Smith    | jane@ex.com     | +91...   | Agent | Blocked| ... |
+------------------------------------------------------------------+
| Showing 1-50 of 1,234          [ < 1 2 3 ... 25 > ]             |
+------------------------------------------------------------------+
```

**Action Menu (...):** View, Edit, Block/Unblock, Delete (with confirmation dialog)

**Detail View** (`/users/{id}`):
- Header: name, avatar placeholder, role badge, status indicator
- Tabs: **Profile** | **Activity** | **Permissions**
- Profile tab: editable fields (name, phone, role dropdown, active toggle)
- Activity tab: audit log filtered by this user
- If user is an agent: link to their agent profile

---

### 5.3 Agents (`/agents`, `/agents/{id}`)

**List View:**

```
+------------------------------------------------------------------+
| Agents                                                           |
+------------------------------------------------------------------+
| Search: [_______________]  Status: [All v] [Pending|Approved|...] |
+------------------------------------------------------------------+
| Agent        | Agency     | License | Rating | Status    | Action |
|--------------|------------|---------|--------|-----------|--------|
| John Doe     | XYZ Realty | LIC123  | 4.5    | Approved  | ...    |
| New Agent    | -          | LIC456  | -      | Pending   | Review |
+------------------------------------------------------------------+
```

**Status Filter Tabs:** All | Pending | Approved | Rejected | Suspended | Blacklisted

**Detail View** (`/agents/{id}`):
- Header: agent name, agency, rating stars, verification status badge
- Linked user profile
- Stats row: Properties (clickable -> filtered list), Builders, Projects
- Verification section: notes, verified by, verified at
- Actions: Approve, Reject (with reason modal), Suspend, Blacklist

---

### 5.4 Properties (`/properties`, `/properties/{id}`)

**List View:**

```
+------------------------------------------------------------------+
| Properties                                   [ + Add Property ]  |
+------------------------------------------------------------------+
| Search: [________]  City: [All v]  Status: [All v]  Approval: [] |
+------------------------------------------------------------------+
| Title         | Type   | City    | Price    | Approval | Status  |
|---------------|--------|---------|----------|----------|---------|
| 3BHK Flat...  | Sale   | Mumbai  | 85L     | Approved | Active  |
| Villa in...   | Sale   | Pune    | 2.1Cr   | Pending  | Draft   |
| 2BHK for...   | Rent   | Delhi   | 25K/mo  | Rejected | -       |
+------------------------------------------------------------------+
```

**Approval Status Color Coding:**
- `draft` - Gray
- `submitted` - Blue
- `under_review` - Amber/Orange
- `approved` - Green
- `rejected` - Red

**Detail View** (`/properties/{id}`):
- Tabbed layout: **Overview** | **Media** | **Location** | **Activity** | **Approval History**
- Overview: all property fields organized in sections (Basic Info, Pricing, Dimensions, Features, Amenities)
- Media: image gallery grid (S3-backed), video URL embed
- Location: address details + map placeholder (future: embedded map)
- Activity: audit trail for this property
- Approval History: timeline showing draft -> submitted -> reviewed with timestamps, reviewer, and notes

**Admin Actions Panel** (sticky at bottom or right sidebar):
- Submit for Review (if draft)
- Approve / Reject (with reason) (if submitted/under_review)
- Flag / Unflag (with reason)
- Publish / Unpublish toggle
- Delete (soft delete with confirmation)

---

### 5.5 Builders (`/builders`, `/builders/{id}`)

**List View:** Standard data table with name, email, phone, established year, active status, project count.

**Detail View:**
- Builder info card
- Projects list (inline table linking to `/projects/{id}`)
- Created by agent (link)
- Moderation actions: Block, Blacklist (admin only)

---

### 5.6 Projects (`/projects`, `/projects/{id}`)

**List View:** Name, builder (linked), city, RERA ID, status, construction status, units.

**Detail View:**
- Project info card with all fields
- Associated builder (linked card)
- Properties in this project (inline table with count)
- Location details
- Timeline: launch date, possession date

---

### 5.7 Approvals (`/approvals/agents`, `/approvals/properties`)

**Queue-Driven Review Screen** (high-efficiency design for admins processing approvals):

```
+------------------------------------------------------------------+
| Approvals    [Agent Approvals]  [Property Approvals]             |
+------------------------------------------------------------------+
| Queue (12 pending)        |  Detail Preview                     |
|                           |                                     |
| > Agent: New Agent 1      |  Name: New Agent 1                  |
|   Submitted: 2h ago      |  Agency: XYZ Realty                 |
|                           |  License: LIC-456                   |
| Agent: New Agent 2        |  Experience: 5 years                |
|   Submitted: 5h ago      |  User: john@example.com             |
|                           |                                     |
| Agent: New Agent 3        |  Documents: [View License]          |
|   Submitted: 1d ago      |                                     |
|                           |  +----------+  +----------+        |
|                           |  | Approve  |  | Reject   |        |
|                           |  +----------+  +----------+        |
|                           |                                     |
|                           |  Notes: [_________________]        |
+------------------------------------------------------------------+
```

**Behavior:**
- Left panel: scrollable queue list, sorted by submission time (oldest first)
- Right panel: full detail of selected item
- After Approve/Reject: auto-advance to next item in queue
- Reject requires a reason (modal or inline text field)
- Badge updates in real-time as queue shrinks
- Empty state: "All caught up! No pending approvals."

---

### 5.8 Audit Logs (`/audit-logs`)

```
+------------------------------------------------------------------+
| Audit Logs                                      [ Export CSV ]   |
+------------------------------------------------------------------+
| Entity Type: [All v]  Performed By: [All v]  Date: [From] [To]  |
+------------------------------------------------------------------+
| Action       | Entity     | Entity ID | By         | Date       |
|--------------|------------|-----------|------------|------------|
| Updated      | Property   | #12345    | Admin John | 2 min ago  |
| Approved     | Agent      | #789      | Admin Jane | 1 hour ago |
| Created      | User       | #456      | System     | 3 hours    |
+------------------------------------------------------------------+
```

- Expandable rows to show full `Details` JSON (formatted)
- Click entity ID to navigate to that entity's detail page
- Click performer to navigate to user detail

---

### 5.9 Roles & Permissions (`/roles`, `/roles/{id}`)

**Roles List:**
- Card-based layout showing each role with name, description, user count, active status
- Click to edit

**Permission Matrix** (`/roles/{id}`):

```
+------------------------------------------------------------------+
| Role: Admin                                      [ Save ]        |
+------------------------------------------------------------------+
| Module          | Read | Create | Update | Delete | Manage | Exp |
|-----------------|------|--------|--------|--------|--------|-----|
| USER_MGMT       | [x]  | [x]   | [x]   | [ ]   | [x]   | [x] |
| PROPERTY_MGMT   | [x]  | [x]   | [x]   | [x]   | [x]   | [x] |
| AGENT_MGMT      | [x]  | [x]   | [x]   | [ ]   | [x]   | [ ] |
| APPROVALS       | [x]  | [ ]   | [ ]   | [ ]   | [x]   | [ ] |
| AUDIT_LOGS      | [x]  | [ ]   | [ ]   | [ ]   | [ ]   | [x] |
| ...             |      |        |        |        |        |     |
+------------------------------------------------------------------+
```

- Checkbox grid for all 10 modules x 6 permissions
- "Select All" per row and per column
- Save calls permission assignment endpoint
- SuperAdmin role: all checkboxes checked and disabled (cannot be modified)

---

### 5.10 Settings (`/settings`)

Future phase. Placeholder page with:
- System configuration (future)
- Notification rules (future)
- Feature flags (future)

---

## 6. Shared Components

### 6.1 Component Library

```
Components/
  Layout/
    AppShell.razor            - Top bar + sidebar + content area
    Sidebar.razor             - Dynamic menu rendering
    TopBar.razor              - Logo, search, notifications, user menu
    Breadcrumb.razor          - Auto-generated breadcrumbs

  Data/
    DataTable.razor           - Server-side paginated table
    Pagination.razor          - Page navigation with size selector
    SearchBar.razor           - Debounced search input
    FilterDropdown.razor      - Dropdown filter with options
    StatusBadge.razor         - Color-coded status pill
    EmptyState.razor          - "No data" illustration + message

  Forms/
    FormField.razor           - Label + input + validation message
    OtpInput.razor            - 6-digit OTP input with auto-advance
    ConfirmDialog.razor       - "Are you sure?" modal
    ReasonDialog.razor        - Modal with text area for rejection reason

  Feedback/
    Toast.razor               - Success/error/info notifications
    LoadingSpinner.razor      - Full-page and inline spinners
    SkeletonLoader.razor      - Placeholder shimmer while loading

  Auth/
    PermissionGuard.razor     - Conditional render based on permission
    AuthorizedPage.razor      - Base component that checks page access
    LoginRedirect.razor       - Redirects to /login if not authenticated
```

### 6.2 DataTable Component (Core Reusable Component)

Every list page uses the same DataTable component:

```razor
<DataTable TItem="UserResponse"
           ApiUrl="/api/admin/users"
           Columns="columns"
           Searchable="true"
           @bind-Page="page"
           @bind-PageSize="pageSize">
    <ActionTemplate Context="user">
        <PermissionGuard Module="USER_MGMT" Permission="CanUpdate">
            <button @onclick="() => Edit(user)">Edit</button>
        </PermissionGuard>
    </ActionTemplate>
</DataTable>
```

Features:
- Generic `TItem` parameter
- Server-side pagination (calls API with page/pageSize query params)
- Column definitions (header, property path, sortable, width)
- Built-in search bar with debounce (300ms)
- Loading skeleton while fetching
- Empty state when no results
- Action column with custom template

---

## 7. Services Layer (WASM)

```
Services/
  ApiClient.cs                - Base HttpClient wrapper with auth headers
  AuthService.cs              - OTP login, token refresh, logout
  AuthStateProvider.cs        - Blazor AuthenticationStateProvider
  TokenService.cs             - localStorage JWT management
  PermissionService.cs        - Client-side permission cache & checks

  UserService.cs              - /api/admin/users endpoints
  AgentService.cs             - /api/agents endpoints
  PropertyService.cs          - /api/properties endpoints
  BuilderService.cs           - /api/builders endpoints
  ProjectService.cs           - /api/projects endpoints
  RoleService.cs              - /api/admin/roles endpoints
  ModuleService.cs            - /api/admin/modules endpoints
  ApprovalService.cs          - /api/admin/approvals endpoints
  AuditLogService.cs          - /api/admin/audit-logs endpoints
  DashboardService.cs         - /api/admin/dashboard endpoints
```

### 7.1 ApiClient Pattern

```csharp
// All service methods return ApiResult<T> to handle errors uniformly
public record ApiResult<T>(bool Success, T? Data, string? Error, int StatusCode);

// Example service method:
public async Task<ApiResult<PagedResult<UserResponse>>> GetUsersAsync(
    int page = 1, int pageSize = 50, string? search = null, bool? isActive = null)
{
    var url = $"/api/admin/users?page={page}&pageSize={pageSize}";
    if (search != null) url += $"&search={Uri.EscapeDataString(search)}";
    if (isActive != null) url += $"&isActive={isActive}";
    return await _client.GetAsync<PagedResult<UserResponse>>(url);
}
```

---

## 8. Route Structure

```
/login                        - OTP login page (public)

/dashboard                    - Dashboard (default after login)

/users                        - User list
/users/{id}                   - User detail

/agents                       - Agent list
/agents/{id}                  - Agent detail

/properties                   - Property list
/properties/{id}              - Property detail
/properties/create            - Create property form

/builders                     - Builder list
/builders/{id}                - Builder detail

/projects                     - Project list
/projects/{id}                - Project detail

/approvals/agents             - Agent approval queue
/approvals/properties         - Property approval queue

/analytics                    - Analytics overview (future)
/analytics/agents             - Agent performance (future)
/analytics/properties         - Property insights (future)

/audit-logs                   - Audit log viewer

/roles                        - Role list
/roles/{id}                   - Role detail + permission matrix

/settings                     - System settings (future)

/access-denied                - Shown when user lacks permission
/not-found                    - 404 page
```

---

## 9. Theming Architecture

### 9.1 CSS Custom Properties (Design Token Ready)

All visual values are defined as CSS custom properties in a single theme file. This makes it trivial to swap themes from Figma/Lovable exports later.

```css
:root {
    /* Primary palette - replace with Figma values later */
    --color-primary: #2563eb;
    --color-primary-hover: #1d4ed8;
    --color-primary-light: #eff6ff;

    /* Neutral palette */
    --color-bg: #f8fafc;
    --color-surface: #ffffff;
    --color-border: #e2e8f0;
    --color-text-primary: #0f172a;
    --color-text-secondary: #64748b;
    --color-text-muted: #94a3b8;

    /* Semantic colors */
    --color-success: #16a34a;
    --color-warning: #f59e0b;
    --color-danger: #dc2626;
    --color-info: #0ea5e9;

    /* Status-specific (for badges) */
    --color-status-draft: #94a3b8;
    --color-status-pending: #f59e0b;
    --color-status-approved: #16a34a;
    --color-status-rejected: #dc2626;
    --color-status-blocked: #7c3aed;

    /* Layout */
    --sidebar-width: 260px;
    --sidebar-collapsed-width: 56px;
    --topbar-height: 56px;

    /* Typography */
    --font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
    --font-size-xs: 0.75rem;
    --font-size-sm: 0.875rem;
    --font-size-base: 1rem;
    --font-size-lg: 1.125rem;
    --font-size-xl: 1.25rem;
    --font-size-2xl: 1.5rem;

    /* Spacing scale */
    --space-1: 0.25rem;
    --space-2: 0.5rem;
    --space-3: 0.75rem;
    --space-4: 1rem;
    --space-6: 1.5rem;
    --space-8: 2rem;

    /* Borders & Shadows */
    --radius-sm: 0.25rem;
    --radius-md: 0.5rem;
    --radius-lg: 0.75rem;
    --shadow-sm: 0 1px 2px rgba(0,0,0,0.05);
    --shadow-md: 0 4px 6px rgba(0,0,0,0.07);
    --shadow-lg: 0 10px 15px rgba(0,0,0,0.1);
}
```

### 9.2 Theme File Structure

```
wwwroot/css/
  tokens.css          - CSS custom properties (design tokens)
  reset.css           - Minimal CSS reset
  layout.css          - Shell, sidebar, topbar layout
  components.css      - DataTable, badges, buttons, cards, forms
  pages.css           - Page-specific overrides (minimal)
  responsive.css      - Breakpoint-specific rules
  app.css             - Entry point that @imports all above
```

### 9.3 Applying a Figma/Lovable Theme Later

When the design is ready:
1. Export color tokens from Figma
2. Replace values in `tokens.css`
3. Add any component-specific overrides
4. No Razor/C# changes needed

---

## 10. Responsive Breakpoints

| Breakpoint | Width | Layout Changes |
|------------|-------|---------------|
| Mobile | < 768px | Sidebar hidden (hamburger overlay), single column, stacked cards, table horizontal scroll |
| Tablet | 768-1024px | Sidebar overlay, 2-column dashboard cards, table fits |
| Desktop | > 1024px | Sidebar persistent, 4-column dashboard cards, full table |

---

## 11. Implementation Phases

### Phase 1: Foundation (Week 1)
1. Auth infrastructure: `AuthService`, `TokenService`, `AuthStateProvider`, `ApiClient`
2. Login page with OTP flow
3. App shell: `TopBar`, `Sidebar`, `Breadcrumb`
4. Permission system: `PermissionGuard`, sidebar filtering
5. CSS tokens and base layout styles
6. Route guards (redirect to `/login` if unauthenticated)

### Phase 2: Core Pages (Week 2)
1. Dashboard page with metric cards
2. `DataTable` component (generic, reusable)
3. Users list + detail page
4. Agents list + detail page
5. Properties list + detail page (tabbed)

### Phase 3: Operational Features (Week 3)
1. Approval queue screens (agent + property)
2. Builders list + detail
3. Projects list + detail
4. Create/Edit forms for properties
5. `ConfirmDialog`, `ReasonDialog`, `Toast` components

### Phase 4: Admin Features (Week 4)
1. Roles & Permissions matrix page
2. Audit Logs viewer with filters
3. Export to CSV functionality
4. Cross-module navigation links (agent -> properties, property -> agent)
5. Settings placeholder page

### Phase 5: Polish & Theme (When Figma Ready)
1. Apply Figma/Lovable design tokens
2. Custom icons and illustrations
3. Empty states and loading animations
4. Dark mode support (optional)
5. Analytics pages (when backend analytics endpoints are ready)

---

## 12. Technical Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| CSS Approach | CSS Custom Properties (no framework) | Theme-ready, no dependency, swap Figma tokens later |
| Component Library | None initially | Avoid lock-in, build what we need, adopt MudBlazor later if needed |
| Icons | SVG sprite or Lucide icons (CDN) | Lightweight, consistent, no package needed |
| State Management | Cascading parameters + services | Blazor-native, no extra libraries |
| HTTP Client | Typed HttpClient with DelegatingHandler | Clean auth injection, testable |
| Token Storage | localStorage via JS interop | Standard WASM pattern, survives page refresh |
| Form Validation | DataAnnotations + FluentValidation (shared) | Reuse Application layer validators |
| Tables | Custom DataTable component | Server-side pagination, consistent across all pages |

---

## 13. File Structure (Final)

```
src/RealEstate.Admin/
  Program.cs
  App.razor
  _Imports.razor

  Auth/
    AuthStateProvider.cs
    AuthorizedPageBase.cs

  Services/
    ApiClient.cs
    AuthService.cs
    TokenService.cs
    PermissionService.cs
    UserService.cs
    AgentService.cs
    PropertyService.cs
    BuilderService.cs
    ProjectService.cs
    RoleService.cs
    ModuleService.cs
    ApprovalService.cs
    AuditLogService.cs
    DashboardService.cs

  Components/
    Layout/
      AppShell.razor
      Sidebar.razor
      TopBar.razor
      Breadcrumb.razor
    Data/
      DataTable.razor
      Pagination.razor
      SearchBar.razor
      FilterDropdown.razor
      StatusBadge.razor
      EmptyState.razor
    Forms/
      FormField.razor
      OtpInput.razor
      ConfirmDialog.razor
      ReasonDialog.razor
    Feedback/
      Toast.razor
      LoadingSpinner.razor
      SkeletonLoader.razor
    Auth/
      PermissionGuard.razor
      LoginRedirect.razor

  Pages/
    Login.razor
    AccessDenied.razor
    NotFound.razor
    Dashboard/
      Dashboard.razor
    Users/
      UserList.razor
      UserDetail.razor
    Agents/
      AgentList.razor
      AgentDetail.razor
    Properties/
      PropertyList.razor
      PropertyDetail.razor
      PropertyForm.razor
    Builders/
      BuilderList.razor
      BuilderDetail.razor
    Projects/
      ProjectList.razor
      ProjectDetail.razor
    Approvals/
      AgentApprovals.razor
      PropertyApprovals.razor
    AuditLogs/
      AuditLogList.razor
    Roles/
      RoleList.razor
      RoleDetail.razor
    Settings/
      Settings.razor
    Analytics/
      AnalyticsOverview.razor

  wwwroot/
    index.html
    css/
      tokens.css
      reset.css
      layout.css
      components.css
      pages.css
      responsive.css
      app.css
    images/
      logo.svg
      empty-state.svg
```

---

## 14. API Endpoint Reference (for WASM Services)

| Service | Method | HTTP | Endpoint |
|---------|--------|------|----------|
| AuthService | RequestOtp | POST | `/api/auth/otp/request` |
| AuthService | VerifyOtp | POST | `/api/auth/otp/verify` |
| AuthService | RefreshToken | POST | `/api/auth/refresh` |
| DashboardService | GetMetrics | GET | `/api/admin/dashboard/metrics` |
| UserService | GetUsers | GET | `/api/admin/users?page&pageSize&search&isActive` |
| UserService | GetUserById | GET | `/api/admin/users/{id}` |
| UserService | UpdateUser | PUT | `/api/admin/users/{id}` |
| UserService | DeleteUser | DELETE | `/api/admin/users/{id}` |
| UserService | BlockUser | POST | `/api/admin/users/{id}/block` |
| UserService | UnblockUser | POST | `/api/admin/users/{id}/unblock` |
| AgentService | GetAgents | GET | `/api/agents?page&pageSize&status` |
| AgentService | GetAgentById | GET | `/api/agents/{id}` |
| AgentService | CreateAgent | POST | `/api/agents` |
| AgentService | UpdateAgent | PUT | `/api/agents/{id}` |
| PropertyService | GetProperties | GET | `/api/properties?page&pageSize&city&isPublished&approvalStatus` |
| PropertyService | GetPropertyById | GET | `/api/properties/{id}` |
| PropertyService | CreateProperty | POST | `/api/properties` |
| PropertyService | UpdateProperty | PUT | `/api/properties/{id}` |
| PropertyService | DeleteProperty | DELETE | `/api/properties/{id}` |
| BuilderService | GetBuilders | GET | `/api/builders?page&pageSize&active` |
| BuilderService | GetBuilderById | GET | `/api/builders/{id}` |
| BuilderService | CreateBuilder | POST | `/api/builders` |
| BuilderService | UpdateBuilder | PUT | `/api/builders/{id}` |
| ProjectService | GetProjects | GET | `/api/projects?page&pageSize&builderId` |
| ProjectService | GetProjectById | GET | `/api/projects/{id}` |
| ProjectService | CreateProject | POST | `/api/projects` |
| ProjectService | UpdateProject | PUT | `/api/projects/{id}` |
| RoleService | GetRoles | GET | `/api/admin/roles` |
| RoleService | GetRoleById | GET | `/api/admin/roles/{id}` |
| RoleService | CreateRole | POST | `/api/admin/roles` |
| RoleService | UpdateRole | PUT | `/api/admin/roles/{id}` |
| RoleService | DeleteRole | DELETE | `/api/admin/roles/{id}` |
| ModuleService | GetModules | GET | `/api/admin/modules` |
| ModuleService | GetModuleById | GET | `/api/admin/modules/{id}` |
| ModuleService | CreateModule | POST | `/api/admin/modules` |
| ModuleService | UpdateModule | PUT | `/api/admin/modules/{id}` |
| ModuleService | DeleteModule | DELETE | `/api/admin/modules/{id}` |
| ApprovalService | SubmitProperty | POST | `/api/admin/approvals/properties/{id}/submit` |
| ApprovalService | ApproveProperty | POST | `/api/admin/approvals/properties/{id}/approve` |
| ApprovalService | RejectProperty | POST | `/api/admin/approvals/properties/{id}/reject` |
| ApprovalService | ApproveAgent | POST | `/api/admin/approvals/agents/{id}/approve` |
| ApprovalService | RejectAgent | POST | `/api/admin/approvals/agents/{id}/reject` |
| AuditLogService | GetLogs | GET | `/api/admin/audit-logs?page&pageSize&entityType&performedBy` |
| AuditLogService | GetEntityTrail | GET | `/api/admin/audit-logs/{entityType}/{entityId}` |

---

## 15. Improvements Over Original Navigation Map

| Original Design | Improvement | Reason |
|-----------------|-------------|--------|
| Blazor Server | Blazor WASM | Better offline resilience, CDN-cacheable, reduced server load |
| Swagger-style login | OTP-only login (Email/Mobile) | Matches actual auth backend, no passwords to manage |
| Static sidebar | Permission-driven dynamic sidebar | Only shows modules the user can access |
| Separate Analytics section | Dashboard-integrated metrics | Analytics endpoints don't exist yet; metrics on dashboard suffice for now |
| Change Requests (Approvals) | Removed for now | No backend support; can add when workflow is built |
| Modal-based editing | Dedicated detail pages with inline editing | Better UX for complex entities like Properties with 50+ fields |
| No breadcrumbs | Auto-generated breadcrumbs | Prevents navigation confusion per the original doc |
| Bootstrap CSS | CSS Custom Properties (design tokens) | Theme-ready for Figma/Lovable, no framework dependency |
| Queue-driven approvals | Split-pane queue (list + detail) | Matches the original doc's "queue-driven high-efficiency" design |
| No global search | Search bar in top bar (future) | Cross-entity search improves admin productivity |
| Separate Builders/Projects pages | Added to sidebar under their own items | These are distinct entities in the backend, deserve their own management pages |
