# RBAC Quick Reference

## 🎯 What Changed

### ✅ Role.cs
- ✅ Added `RoleType` enum with integer values (Guest=0, User=1, Agent=5, Admin=10, SuperAdmin=100)
- ✅ Changed `Name` property from `RoleType` enum to `string` for display name
- ✅ Added `RoleType` property to store the enum value
- ✅ Added `IsActive` flag for soft-deleting roles
- ✅ Added navigation property to `Users` and `Permissions`
- ✅ Created `Module` entity for organizing features
- ✅ Renamed `RoleAccess` → `RolePermission` with 5 permission types:
  - `CanRead`, `CanCreate`, `CanUpdate`, `CanDelete`, `CanManage`

### ✅ User.cs
- ✅ Changed `Role` from `string?` to `Guid RoleId` (required FK)
- ✅ Added navigation property `public Role Role { get; set; } = null!;`
- ✅ Added `IsActive` flag

### ✅ AppDbContext.cs
- ✅ Added `DbSet<Role>`, `DbSet<Module>`, `DbSet<RolePermission>`
- ✅ Configured relationships and indexes
- ✅ Added seed data for 5 default roles
- ✅ Added seed data for 4 default modules

### ✅ AuthService.cs
- ✅ Updated all login methods to assign default `User` role to new users
- ✅ Include `Role` when loading users
- ✅ Updated `UserInfo` response to include role information

### ✅ AuthModels.cs
- ✅ Updated `UserInfo` record to include `Role` (name) and `RoleLevel` (int)

### ✅ New Files
- ✅ `Infrastructure/Authorization/PermissionService.cs` - Service for checking permissions
- ✅ `RBAC-GUIDE.md` - Comprehensive documentation

---

## 📊 Database Structure

```
┌─────────────┐         ┌──────────────────┐         ┌─────────────┐
│    Roles    │◄────────│  RolePermissions │────────►│   Modules   │
├─────────────┤         ├──────────────────┤         ├─────────────┤
│ Id          │         │ Id               │         │ Id          │
│ RoleType ⚡ │         │ RoleId (FK)      │         │ Code        │
│ Name        │         │ ModuleId (FK)    │         │ Name        │
│ Description │         │ CanRead          │         │ Description │
│ IsActive    │         │ CanCreate        │         │ IsActive    │
└─────────────┘         │ CanUpdate        │         └─────────────┘
       ▲                │ CanDelete        │
       │                │ CanManage        │
       │                └──────────────────┘
       │
       │ FK: RoleId (required)
       │
┌─────────────┐
│    Users    │
├─────────────┤
│ Id          │
│ Email       │
│ RoleId ⚡   │
│ IsActive    │
└─────────────┘
```

---

## 🔑 Role Hierarchy

```
SuperAdmin (100) ──┐
                   │
Admin (10) ────────┤  Higher privilege level
                   │
Agent (5) ─────────┤
                   │
User (1) ──────────┤
                   │
Guest (0) ─────────┘  Lower privilege level
```

---

## 🎨 Default Modules

```
USER_MGMT     → User Management
PROPERTY      → Property Management
ANALYTICS     → Analytics & Reporting
SETTINGS      → System Settings
```

---

## 💡 Common Tasks

### Check if user can create properties
```csharp
if (await _permissionService.CanCreateAsync(userId, "PROPERTY"))
{
    // Allow creation
}
```

### Check if user is at least Admin
```csharp
if (await _permissionService.HasMinimumRoleAsync(userId, RoleType.Admin))
{
    // User is Admin or SuperAdmin
}
```

### Promote user to Agent
```csharp
var user = await _context.Users.FindAsync(userId);
var agentRole = await _context.Roles.FirstAsync(r => r.RoleType == RoleType.Agent);
user.RoleId = agentRole.Id;
await _context.SaveChangesAsync();
```

### Grant permission to a role
```csharp
var permission = new RolePermission
{
    Id = Guid.NewGuid(),
    RoleId = agentRoleId,
    ModuleId = propertyModuleId,
    CanRead = true,
    CanCreate = true,
    CanUpdate = true,
    CanDelete = false,
    CanManage = false
};
_context.RolePermissions.Add(permission);
await _context.SaveChangesAsync();
```

---

## 🚀 Next Steps

1. **Apply migration:**
   ```bash
   dotnet ef database update
   ```

2. **Seed permissions** (configure based on your requirements)

3. **Protect endpoints** using `IPermissionService`

4. **Test the system** with different user roles

---

## 📝 API Response Example

```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "name": "John Doe",
    "role": "User",           ← Display name
    "roleLevel": 1            ← Numeric value for UI logic
  }
}
```

**Client-side usage:**
- Show admin panel: `if (user.roleLevel >= 10)`
- Show "AGENT" badge: `if (user.roleLevel === 5)`
- Enable property creation: `if (user.roleLevel >= 5)`

---

## ⚠️ Important Notes

- **Default role:** All new users get `RoleType.User` (level 1)
- **Role deletion:** Prevented if users exist with that role
- **Permission checking:** Always use `IPermissionService`, not raw DB queries
- **Hierarchy:** Use `HasMinimumRoleAsync()` to check "at least this role level"
