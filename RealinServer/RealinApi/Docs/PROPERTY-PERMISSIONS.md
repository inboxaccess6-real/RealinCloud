# Property App Permission Guide

## 🎯 Your Use Case: Agents vs. Buyers

### ✅ Solution: Use Existing Roles, No New User Type Needed!

```
┌──────────────────────────────────────────────────────────────┐
│  GUEST (0)                                                   │
│  - Browse properties (read-only)                             │
│  - No bookmarks, no account required                         │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  USER (1) - Regular buyers/renters 👥                        │
│  ✅ View all properties                                      │
│  ✅ Create & manage their own bookmarks                      │
│  ❌ Cannot create properties                                 │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  AGENT (5) - Real estate agents 🏢                           │
│  ✅ View all properties                                      │
│  ✅ Create new property listings                             │
│  ✅ Update their own listings                                │
│  ✅ View analytics for their listings                        │
│  ✅ Create bookmarks                                         │
│  ❌ Cannot delete properties                                 │
│  ❌ Cannot approve/reject others' listings                   │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  ADMIN (10) - System administrators 👨‍💼                       │
│  ✅ Full property management (approve, reject, delete)       │
│  ✅ Manage users and assign roles                            │
│  ✅ View all analytics                                       │
│  ✅ Configure system settings                                │
└──────────────────────────────────────────────────────────────┘
```

---

## 📊 Module Structure

Your app now has **6 modules** (not 4):

| Module Code | Name | Who Uses It |
|------------|------|-------------|
| `PROPERTY_BROWSE` | Property Browsing | Everyone (Guest, User, Agent, Admin) |
| `PROPERTY_MANAGE` | Property Management | Agents & Admins only |
| `BOOKMARKS` | Bookmarks & Favorites | Users & Agents |
| `USER_MGMT` | User Management | Admins only |
| `ANALYTICS` | Analytics & Reporting | Agents (own) & Admins (all) |
| `SETTINGS` | System Settings | Admins only |

---

## 🔐 Permission Matrix

| Action | Guest | User (Buyer) | Agent | Admin |
|--------|-------|--------------|-------|-------|
| **Browse Properties** | ✅ Read | ✅ Read | ✅ Read | ✅ Full |
| **Create Property** | ❌ | ❌ | ✅ | ✅ |
| **Edit Own Property** | ❌ | ❌ | ✅ | ✅ |
| **Delete Property** | ❌ | ❌ | ❌ | ✅ |
| **Approve Listings** | ❌ | ❌ | ❌ | ✅ |
| **Create Bookmarks** | ❌ | ✅ | ✅ | ✅ |
| **Edit Own Bookmarks** | ❌ | ✅ | ✅ | ✅ |
| **View Own Analytics** | ❌ | ❌ | ✅ | ✅ |
| **View All Analytics** | ❌ | ❌ | ❌ | ✅ |
| **Manage Users** | ❌ | ❌ | ❌ | ✅ |

---

## 💻 Code Examples

### Example 1: Property Creation Endpoint

```csharp
app.MapPost("/api/properties", async (
    HttpContext context,
    CreatePropertyRequest request,
    IPermissionService permissionService) =>
{
    var userId = context.GetUserId();
    if (userId == null) return Results.Unauthorized();

    // Only Agents and Admins can create properties
    if (!await permissionService.CanCreateAsync(userId.Value, "PROPERTY_MANAGE"))
    {
        return Results.Forbid(); // 403 - User sees "You need to be an agent"
    }

    // Create the property...
    // ...
}).RequireAuthorization();
```

### Example 2: Bookmark Creation Endpoint

```csharp
app.MapPost("/api/bookmarks", async (
    HttpContext context,
    CreateBookmarkRequest request,
    IPermissionService permissionService) =>
{
    var userId = context.GetUserId();
    if (userId == null) return Results.Unauthorized();

    // Users, Agents, and Admins can create bookmarks
    if (!await permissionService.CanCreateAsync(userId.Value, "BOOKMARKS"))
    {
        return Results.Forbid(); // 403 - Guest users cannot bookmark
    }

    // Create bookmark for this user
    var bookmark = new Bookmark
    {
        UserId = userId.Value,
        PropertyId = request.PropertyId,
        // ...
    };
    
    // ...
}).RequireAuthorization();
```

### Example 3: Property Browsing (No Auth Required)

```csharp
app.MapGet("/api/properties", async (
    int page,
    int pageSize,
    AppDbContext context) =>
{
    // Public endpoint - no authentication required
    // Everyone (including guests) can browse
    
    var properties = await context.Properties
        .Where(p => p.IsPublished && p.IsActive)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return Results.Ok(properties);
}); // No .RequireAuthorization() needed
```

### Example 4: Check Role for UI Display

```csharp
app.MapGet("/api/auth/me", async (
    HttpContext context,
    AppDbContext dbContext) =>
{
    var userId = context.GetUserId();
    if (userId == null) return Results.Unauthorized();
    
    var user = await dbContext.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Id == userId);
    
    if (user == null) return Results.NotFound();
    
    return Results.Ok(new
    {
        user.Id,
        user.Email,
        user.Name,
        Role = user.Role.Name,
        RoleLevel = (int)user.Role.RoleType,
        Permissions = new
        {
            CanCreateProperties = user.Role.RoleType >= RoleType.Agent,
            CanManageBookmarks = user.Role.RoleType >= RoleType.User,
            CanViewAnalytics = user.Role.RoleType >= RoleType.Agent,
            IsAdmin = user.Role.RoleType >= RoleType.Admin
        }
    });
}).RequireAuthorization();
```

---

## 🚀 Setup Steps

### 1. Apply Migration
```bash
dotnet ef migrations add InitialCreateWithRBAC
dotnet ef database update
```

### 2. Run the App
```bash
dotnet run
```

The app will **automatically seed permissions** on first run (development mode only).

You'll see:
```
✅ Successfully seeded 18 default permissions!

Permission Summary:
- Guest: Browse properties only
- User: Browse properties + manage own bookmarks
- Agent: Create/edit properties + analytics
- Admin: Full property & user management
- SuperAdmin: Full system access
```

### 3. Test User Flow

**Create a regular user (buyer):**
```bash
# User signs up via Google/Apple/OTP
# Automatically gets RoleType.User (level 1)
POST /api/auth/google
```

**Promote user to Agent:**
```csharp
var user = await context.Users.FindAsync(userId);
var agentRole = await context.Roles.FirstAsync(r => r.RoleType == RoleType.Agent);
user.RoleId = agentRole.Id;
await context.SaveChangesAsync();
```

---

## 🎨 Client-Side Usage

Your mobile app receives this in login response:

```json
{
  "user": {
    "id": "...",
    "email": "john@example.com",
    "role": "User",
    "roleLevel": 1
  }
}
```

**Swift/Flutter UI Logic:**
```swift
// Show "Add Property" button only for agents
if user.roleLevel >= 5 {
    showAddPropertyButton()
}

// Show bookmarks feature for logged-in users
if user.roleLevel >= 1 {
    showBookmarksTab()
}

// Show admin panel for admins
if user.roleLevel >= 10 {
    showAdminPanel()
}
```

---

## ❓ FAQ

### Q: Do I need a "Buyer" role separate from "User"?
**A:** No! The `User` role (level 1) is perfect for buyers/renters. They can:
- View properties
- Create bookmarks
- Manage their favorites

### Q: How do I make someone an Agent?
**A:** Admin updates their `RoleId` to the Agent role:
```csharp
user.RoleId = agentRoleId;
```

### Q: Can Agents view other Agents' properties?
**A:** Yes, they can **view** (CanRead) but cannot **edit** others' listings. Add ownership checks:
```csharp
if (property.CreatedByUserId != userId && !isAdmin)
{
    return Results.Forbid();
}
```

### Q: Can Users bookmark properties without signing up?
**A:** No, bookmarks require authentication. Guests can only browse. This encourages sign-ups!

### Q: What if I want a "Premium User" tier?
**A:** Add a new role:
```csharp
public enum RoleType
{
    Guest = 0,
    User = 1,
    PremiumUser = 3,  // NEW: Between User and Agent
    Agent = 5,
    Admin = 10,
    SuperAdmin = 100
}
```

Then configure permissions for premium features (unlimited bookmarks, advanced search, etc.).

---

## ✅ Summary

Your current RBAC system **perfectly handles** your use case:

✅ **Guest** → Browse only  
✅ **User** → Browse + Bookmark (buyers/renters) ← **This is what you wanted**  
✅ **Agent** → Create/manage properties ← **This is what you wanted**  
✅ **Admin** → Full control  

**No new role needed!** 🎉

The system separates:
- `PROPERTY_BROWSE` (everyone can view)
- `PROPERTY_MANAGE` (only agents can create)
- `BOOKMARKS` (logged-in users can save favorites)

This gives you exactly the functionality you described.
