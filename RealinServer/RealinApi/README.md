# RealinApi - Real Estate Management Platform

A comprehensive real estate management API built with .NET 10.0, featuring advanced authentication, role-based authorization, and property management capabilities.

## 🚀 Tech Stack

- **.NET 10.0** - Latest .NET framework with minimal API architecture
- **PostgreSQL 16** - Primary database
- **Entity Framework Core 9.0** - ORM with code-first migrations
- **JWT Authentication** - Secure token-based authentication
- **Scalar API Documentation** - Modern OpenAPI/Swagger alternative
- **Docker** - Containerized deployment support

## 📦 Key Features

### 🔐 Authentication & Authorization
- **Multi-Provider OAuth**: Google Sign-In, Apple Sign-In
- **OTP Authentication**: Phone number-based login
- **JWT Tokens**: Access tokens with role-based claims
- **Refresh Tokens**: Secure 30-day refresh mechanism
- **Role-Based Access Control (RBAC)**: 5-tier role hierarchy
  - Guest (0) - Browse only
  - User (1) - Standard features + bookmarks
  - Agent (5) - Property management
  - Admin (10) - User & property management
  - SuperAdmin (100) - Full system access

### 🏠 Property Domain
Complete property management system with:
- **Properties**: Full property listings with location, pricing, amenities
- **Projects**: Real estate projects (residential/commercial developments)
- **Builders**: Builder/developer profiles and portfolios
- **Agents**: Real estate agent profiles linked to user accounts
- **Media**: Property images and videos support

### 👥 Admin Management (SuperAdmin Only)
- **User Management**: View, update, delete users with pagination
- **Role Management**: Create, update, delete roles with validation
- **Module Management**: System module configuration
- **Permission Seeding**: Automated default permission setup

### 🔑 Security Features
- **Agent Self-Registration**: Users can only create their own agent profile
- **Duplicate Prevention**: Automatic validation for roles, modules, agents
- **Safe Deletion**: Prevents deletion of roles/modules with dependencies
- **Public Viewing**: Anyone can browse properties, projects, builders, agents
- **Protected Creation**: Only Agent/Admin/SuperAdmin can create resources

## 📊 Database Schema

### Authentication Tables
- `users` - User accounts with OAuth/OTP support
- `roles` - System roles with RoleType enum
- `modules` - Application feature modules
- `role_permissions` - Granular CRUD permissions (read/create/update/delete/manage)
- `refresh_tokens` - JWT refresh token storage
- `otp_sessions` - Temporary OTP verification sessions

### Property Tables
- `agents` - Real estate agent profiles (1:1 with users)
- `builders` - Builder/developer information
- `projects` - Real estate projects (belongs to builder)
- `properties` - Property listings (required agent, optional project)
- `media` - Property images/videos

### Entity Relationships
```
User (1) ------- (1) Agent
Agent (1) ----< (N) Properties
Agent (1) ----< (N) Builders (created_by_agent_id)
Agent (1) ----< (N) Projects (created_by_agent_id)
Builder (1) ----< (N) Projects
Project (1) ----< (N) Properties (optional)
Property (1) ----< (N) Media
```

## 🛠️ Project Structure

```
RealinApi/
├── Common/                      # Shared utilities
│   ├── ApiResponse.cs          # Standard API response wrapper
│   └── ValidationExtensions.cs  # Fluent validation helpers
├── Data/                        # Database layer
│   ├── AppDbContext.cs         # EF Core context
│   ├── SeedPermissions.cs      # Permission seeding logic
│   └── Entities/               # Domain entities
├── Features/                    # Feature-based organization
│   ├── Auth/                   # Authentication endpoints & services
│   │   ├── AuthEndpoints.cs    # Login, register, refresh, OTP
│   │   ├── AuthService.cs      # Auth business logic
│   │   └── Models/             # Auth DTOs
│   ├── Admin/                  # Admin management (SuperAdmin only)
│   │   ├── UserEndpoints.cs    # User CRUD APIs
│   │   ├── RoleEndpoints.cs    # Role CRUD APIs
│   │   ├── ModuleEndpoints.cs  # Module CRUD APIs
│   │   ├── UserService.cs      # User management logic
│   │   ├── RoleService.cs      # Role management logic
│   │   ├── ModuleService.cs    # Module management logic
│   │   └── Models/             # Admin DTOs
│   └── Property/               # Property domain
│       ├── PropertyEndpoints.cs # Property CRUD APIs
│       ├── AgentEndpoints.cs    # Agent CRUD APIs
│       ├── BuilderEndpoints.cs  # Builder CRUD APIs
│       ├── ProjectEndpoints.cs  # Project CRUD APIs
│       ├── PropertyService.cs   # Property business logic
│       ├── AgentService.cs      # Agent business logic
│       ├── BuilderService.cs    # Builder business logic
│       ├── ProjectService.cs    # Project business logic
│       └── Models/              # Property DTOs (separated per entity)
├── Infrastructure/              # Cross-cutting concerns
│   ├── Authentication/
│   │   └── JwtService.cs       # JWT generation & validation
│   ├── Authorization/
│   │   └── PermissionService.cs # Permission checking
│   ├── ExternalServices/
│   │   ├── GoogleAuthService.cs # Google OAuth validation
│   │   └── AppleAuthService.cs  # Apple Sign-In with JWKS
│   └── Messaging/
│       ├── SmsService.cs       # SMS/OTP delivery
│       └── EmailService.cs     # Email notifications
├── Migrations/                  # EF Core migrations
├── Docs/                        # API documentation
└── Program.cs                   # Application entry point
```

## 🔌 API Endpoints

### 🔓 Authentication (`/api/auth`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/register` | Register new user | No |
| POST | `/login/google` | Google Sign-In | No |
| POST | `/login/apple` | Apple Sign-In | No |
| POST | `/login/otp/send` | Send OTP to phone | No |
| POST | `/login/otp/verify` | Verify OTP code | No |
| POST | `/refresh` | Refresh access token | No |
| POST | `/logout` | Invalidate refresh token | Yes |

### 👥 Admin - Users (`/api/admin/users`) - SuperAdmin Only
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List all users (paginated) |
| GET | `/{id}` | Get user by ID |
| PUT | `/{id}` | Update user (role, status, etc.) |
| DELETE | `/{id}` | Delete user |

### 🔐 Admin - Roles (`/api/admin/roles`) - SuperAdmin Only
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List all roles |
| GET | `/{id}` | Get role by ID |
| POST | `/` | Create new role |
| PUT | `/{id}` | Update role |
| DELETE | `/{id}` | Delete role (if no users) |

### 📦 Admin - Modules (`/api/admin/modules`) - SuperAdmin Only
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List all modules |
| GET | `/{id}` | Get module by ID |
| POST | `/` | Create new module |
| PUT | `/{id}` | Update module |
| DELETE | `/{id}` | Delete module (if no permissions) |

### 🏠 Properties (`/api/properties`)
| Method | Endpoint | Description | Auth Required | Roles |
|--------|----------|-------------|---------------|-------|
| GET | `/{id}` | View property details | No | Public |
| POST | `/` | Create property | Yes | Agent, Admin, SuperAdmin |

### 🏗️ Projects (`/api/projects`)
| Method | Endpoint | Description | Auth Required | Roles |
|--------|----------|-------------|---------------|-------|
| GET | `/{id}` | View project details | No | Public |
| POST | `/` | Create project | Yes | Agent, Admin, SuperAdmin |

### 🏢 Builders (`/api/builders`)
| Method | Endpoint | Description | Auth Required | Roles |
|--------|----------|-------------|---------------|-------|
| GET | `/{id}` | View builder details | No | Public |
| POST | `/` | Create builder | Yes | Agent, Admin, SuperAdmin |

### 👤 Agents (`/api/agents`)
| Method | Endpoint | Description | Auth Required | Roles |
|--------|----------|-------------|---------------|-------|
| GET | `/{id}` | View agent details | No | Public |
| GET | `/user/{userId}` | Get agent by user ID | No | Public |
| POST | `/` | Create agent profile | Yes | Agent (self-only) |

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 16
- Docker (optional)

### Environment Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd RealinCloud/RealinServer/RealinApi
```

2. **Configure User Secrets**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=realin_dev;Username=realin;Password=your_password"
dotnet user-secrets set "Jwt:SecretKey" "your-256-bit-secret-key-here"
dotnet user-secrets set "Jwt:Issuer" "RealinApi"
dotnet user-secrets set "Jwt:Audience" "RealinApp"
dotnet user-secrets set "Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Apple:ClientId" "your-apple-client-id"
```

3. **Database Setup**
```bash
# Using Docker
docker-compose up -d

# Or run PostgreSQL locally
# Update connection string accordingly
```

4. **Apply Migrations**
```bash
dotnet ef database update
```

5. **Run the Application**
```bash
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5069
- **HTTPS**: https://localhost:7069
- **API Docs**: http://localhost:5069/scalar/v1

## 📖 API Documentation

Interactive API documentation is available via **Scalar** (modern Swagger alternative):
- Navigate to `/scalar/v1` for interactive API testing
- OpenAPI spec available at `/openapi/v1.json`

## 🔒 Security Best Practices

- ✅ JWT tokens expire after 60 minutes (configurable)
- ✅ Refresh tokens valid for 30 days
- ✅ All sensitive data stored in User Secrets (never in appsettings)
- ✅ Password hashing not implemented (OAuth/OTP only)
- ✅ Apple Sign-In uses JWKS with 24-hour key caching
- ✅ Role-based authorization on all protected endpoints
- ✅ CORS configured (adjust for production)

## 🐳 Docker Support

Build and run with Docker:
```bash
# Development
docker-compose -f docker-compose.dev.yml up

# Production
docker-compose up
```

## 📝 Database Naming Convention

All database objects use **snake_case** naming:
- Tables: `users`, `role_permissions`, `properties`
- Columns: `user_id`, `created_at`, `is_active`
- Consistent across all entities for PostgreSQL best practices

## 🧪 Testing

```bash
# Run tests (when available)
dotnet test

# Check code coverage
dotnet test /p:CollectCoverage=true
```

## 📚 Additional Documentation

- [Architecture Overview](ARCHITECTURE.md)
- [Quick Start Guide](QUICKSTART.md)
- [Docker Guide](DOCKER-QUICK.md)
- [RBAC Guide](Docs/RBAC-GUIDE.md)
- [Secrets Management](Docs/SECRETS.md)
- [Apple Authentication](Docs/APPLE-AUTH.md)

## 🤝 Contributing

1. Create a feature branch
2. Follow existing code structure (feature-based organization)
3. Use snake_case for database entities
4. Add XML documentation for public APIs
5. Update README for new features

## 📄 License

[Your License Here]

## 👨‍💻 Author

Murali Krishna Garapati

---

**Built with ❤️ using .NET 10.0 and PostgreSQL**
