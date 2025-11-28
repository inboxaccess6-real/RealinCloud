# Role-Based Access Control (RBAC) Guide

## Overview

RealinApi implements a comprehensive Role-Based Access Control system with:
- **Hierarchical Roles** with numeric levels
- **Module-based Permissions** for granular access control
- **CRUD + Manage** permissions per module
- **Default role assignment** for new users
- **Permission checking** utilities

---

## Role Hierarchy

Roles are defined with explicit integer values representing privilege levels:

```csharp
public enum RoleType
{
    Guest = 0,        // Unverified or limited access users
    User = 1,         // Standard verified users
    Agent = 5,        // Real estate agents with property management access
    Admin = 10,       // System administrators
    SuperAdmin = 100  // Full system access
}
```

**Higher number = Higher privileges**

### Default Roles (Seeded on Database Creation)

| Role ID | RoleType | Name | Level | Description |
|---------|----------|------|-------|-------------|
| `00000000-0000-0000-0000-000000000001` | Guest | Guest | 0 | Limited access for unverified users |
| `00000000-0000-0000-0000-000000000002` | User | User | 1 | Standard verified user (DEFAULT) |
| `00000000-0000-0000-0000-000000000003` | Agent | Real Estate Agent | 5 | Property management capabilities |
| `00000000-0000-0000-0000-000000000004` | Admin | Administrator | 10 | System administrator |
| `00000000-0000-0000-0000-000000000005` | SuperAdmin | Super Administrator | 100 | Full system access |

---

## Modules

Modules represent application features/areas that can have permissions assigned:

### Default Modules (Seeded on Database Creation)

| Module ID | Code | Name | Description |
|-----------|------|------|-------------|
| `10000000-0000-0000-0000-000000000001` | USER_MGMT | User Management | Manage user accounts, roles, and permissions |
| `10000000-0000-0000-0000-000000000002` | PROPERTY | Property Management | Create, edit, and manage property listings |
| `10000000-0000-0000-0000-000000000003` | ANALYTICS | Analytics & Reporting | View analytics, reports, and business insights |
| `10000000-0000-0000-0000-000000000004` | SETTINGS | System Settings | Configure system-wide settings and preferences |

---

## Permission Types

Each Role-Module combination can have 5 permission flags:

| Permission | Description | Example |
|------------|-------------|---------|
| **CanRead** | View/read data in this module | View property listings |
| **CanCreate** | Create new records | Add new property |
| **CanUpdate** | Modify existing records | Edit property details |
| **CanDelete** | Remove records | Delete property listing |
| **CanManage** | Administrative privileges | Approve listings, manage settings |

---

## Database Schema

### Tables

#### `Roles`
```sql
- Id (UUID, PK)
- RoleType (INT, UNIQUE) -- Enum value (0, 1, 5, 10, 100)
- Name (VARCHAR(100)) -- Display name
- Description (VARCHAR(500))
- IsActive (BOOLEAN, default: true)
- CreatedAt, UpdatedAt (TIMESTAMP)
```

#### `Modules`
```sql
- Id (UUID, PK)
- Code (VARCHAR(50), UNIQUE) -- e.g., "PROPERTY", "USER_MGMT"
- Name (VARCHAR(100))
- Description (VARCHAR(500))
- IsActive (BOOLEAN, default: true)
- CreatedAt, UpdatedAt (TIMESTAMP)
```

#### `RolePermissions`
```sql
- Id (UUID, PK)
- RoleId (UUID, FK → Roles.Id)
- ModuleId (UUID, FK → Modules.Id)
- CanRead, CanCreate, CanUpdate, CanDelete, CanManage (BOOLEAN)
- CreatedAt, UpdatedAt (TIMESTAMP)
- UNIQUE INDEX on (RoleId, ModuleId)
```

#### `Users` (Updated)
```sql
- RoleId (UUID, FK → Roles.Id, REQUIRED)
- IsActive (BOOLEAN, default: true)
- FK constraint: ON DELETE RESTRICT (prevents role deletion if users exist)
```

---

## Usage Examples

### 1. Assign Role to New User (Automatic)

When a user registers via Google/Apple/OTP, they're automatically assigned the **User** role:

```csharp
// In AuthService.cs
var defaultRole = await _context.Roles
    .FirstOrDefaultAsync(r => r.RoleType == RoleType.User);

var user = new User
{
    Email = email,
    RoleId = defaultRole.Id, // Assigned automatically
    // ...
};
```

### 2. Check User Permissions

Using the `IPermissionService`:

```csharp
// Inject IPermissionService
private readonly IPermissionService _permissionService;

// Check specific permission
if (await _permissionService.CanCreateAsync(userId, "PROPERTY"))
{
    // User can create properties
}

// Check role level
if (await _permissionService.HasMinimumRoleAsync(userId, RoleType.Admin))
{
    // User is Admin or SuperAdmin
}

// Get all permissions for a module
var permissions = await _permissionService.GetPermissionsAsync(userId, "PROPERTY");
if (permissions?.CanUpdate == true && permissions?.CanDelete == true)
{
    // User can update AND delete properties
}
```

### 3. Protect Endpoints with Permissions

```csharp
// In your endpoint handler
app.MapPost("/api/properties", async (
    HttpContext context,
    CreatePropertyRequest request,
    IPermissionService permissionService) =>
{
    var userId = context.GetUserId();
    if (userId == null)
    {
        return Results.Unauthorized();
    }

    // Check permission
    if (!await permissionService.CanCreateAsync(userId.Value, "PROPERTY"))
    {
        return Results.Forbid(); // 403 Forbidden
    }

    // User has permission, proceed...
    // ...
}).RequireAuthorization();
```

### 4. Create Role Permissions

```csharp
// Example: Give Agent role CRUD access to PROPERTY module
var agentRole = await _context.Roles
    .FirstOrDefaultAsync(r => r.RoleType == RoleType.Agent);

var propertyModule = await _context.Modules
    .FirstOrDefaultAsync(m => m.Code == "PROPERTY");

var permission = new RolePermission
{
    Id = Guid.NewGuid(),
    RoleId = agentRole.Id,
    ModuleId = propertyModule.Id,
    CanRead = true,
    CanCreate = true,
    CanUpdate = true,
    CanDelete = false, // Agents can't delete
    CanManage = false,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

_context.RolePermissions.Add(permission);
await _context.SaveChangesAsync();
```

### 5. Upgrade User Role

```csharp
// Promote user to Agent
var user = await _context.Users.FindAsync(userId);
var agentRole = await _context.Roles
    .FirstOrDefaultAsync(r => r.RoleType == RoleType.Agent);

user.RoleId = agentRole.Id;
user.UpdatedAt = DateTime.UtcNow;

await _context.SaveChangesAsync();
```

---

## Best Practices

### ✅ DO

1. **Always use `IPermissionService`** for permission checks
2. **Check permissions at the endpoint level** before processing requests
3. **Use `HasMinimumRoleAsync`** for role-level checks (respects hierarchy)
4. **Load Role with User** when authentication data is needed:
   ```csharp
   var user = await _context.Users
       .Include(u => u.Role)
       .FirstOrDefaultAsync(u => u.Id == userId);
   ```
5. **Create unique modules** for each feature area
6. **Set default permissions** for each new role

### ❌ DON'T

1. **Don't hard-code role checks** - use the service
2. **Don't delete roles** with active users (DB constraint prevents this)
3. **Don't bypass permission checks** for "convenience"
4. **Don't create duplicate Role-Module permissions** (unique constraint prevents this)

---

## API Response Changes

The `UserInfo` model now includes role information:

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-11-26T10:00:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "phoneNumber": "+1234567890",
    "name": "John Doe",
    "provider": "Google",
    "role": "User",           // NEW: Role display name
    "roleLevel": 1            // NEW: Numeric privilege level
  }
}
```

Clients can use `roleLevel` to:
- Show/hide UI features (e.g., admin panel visible if `roleLevel >= 10`)
- Display badges (e.g., "AGENT" badge if `roleLevel === 5`)
- Enable/disable actions based on permissions

---

## Migration Steps

### 1. Apply Migration
```bash
dotnet ef database update
```

This creates:
- `Roles` table with 5 default roles
- `Modules` table with 4 default modules
- `RolePermissions` table (empty - configure per your needs)
- Updates `Users` table with `RoleId` column

### 2. Configure Initial Permissions

After migration, seed role permissions based on your requirements:

```csharp
// Example: Give User role read-only access to properties
var userRole = await _context.Roles.FirstAsync(r => r.RoleType == RoleType.User);
var propertyModule = await _context.Modules.FirstAsync(m => m.Code == "PROPERTY");

_context.RolePermissions.Add(new RolePermission
{
    Id = Guid.NewGuid(),
    RoleId = userRole.Id,
    ModuleId = propertyModule.Id,
    CanRead = true,
    CanCreate = false,
    CanUpdate = false,
    CanDelete = false,
    CanManage = false,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
});

await _context.SaveChangesAsync();
```

### 3. Register PermissionService

Add to `Program.cs`:

```csharp
builder.Services.AddScoped<IPermissionService, PermissionService>();
```

---

## Testing Permission System

### SQL Queries for Verification

```sql
-- View all roles
SELECT * FROM "Roles" ORDER BY "RoleType";

-- View all modules
SELECT * FROM "Modules" ORDER BY "Code";

-- View all permissions
SELECT 
    r."Name" AS Role,
    m."Name" AS Module,
    rp."CanRead",
    rp."CanCreate",
    rp."CanUpdate",
    rp."CanDelete",
    rp."CanManage"
FROM "RolePermissions" rp
JOIN "Roles" r ON rp."RoleId" = r."Id"
JOIN "Modules" m ON rp."ModuleId" = m."Id"
ORDER BY r."RoleType", m."Code";

-- View user roles
SELECT 
    u."Email",
    u."Name",
    r."Name" AS Role,
    r."RoleType"
FROM "Users" u
JOIN "Roles" r ON u."RoleId" = r."Id"
ORDER BY r."RoleType" DESC;
```

---

## Summary

Your RBAC implementation now includes:

✅ **Enum-based RoleType** with integer values (0, 1, 5, 10, 100)  
✅ **Role entity** with Name and RoleType fields  
✅ **Module entity** for organizing permissions  
✅ **RolePermission junction table** with 5 permission types  
✅ **User-Role relationship** (required FK)  
✅ **Default role assignment** (User role for new signups)  
✅ **PermissionService** for checking permissions  
✅ **Seeded data** (5 roles, 4 modules)  
✅ **Updated API responses** with role info  
✅ **Database migration** ready to apply  

**Missing/TODO:**
- Permission seed data (configure based on your requirements)
- Custom authorization attributes (optional)
- Role management endpoints (create/update/delete roles)
- Permission management endpoints (assign/revoke permissions)
