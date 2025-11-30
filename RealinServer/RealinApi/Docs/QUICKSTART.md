# Quick Start Guide - RealinApi

## 🔒 **IMPORTANT: Setup Secrets First!**

**Your credentials are now stored securely using .NET User Secrets** - never committed to Git!

### Quick Setup (30 seconds)
```bash
# Automated setup (recommended)
./setup-secrets.sh

# Or manual
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=realin_dev;Username=your_user;Password=your_password"
dotnet user-secrets set "Jwt:SecretKey" "your_secure_secret_key_minimum_64_characters_long"
```

📚 See [SECRETS.md](SECRETS.md) for complete guide | [SECRETS-QUICK.md](SECRETS-QUICK.md) for quick reference

---

## ✅ What's Implemented

Your .NET minimal API is now complete with:

1. **Google OAuth Authentication** - Validates Google ID tokens
2. **Apple OAuth Authentication** - Validates Apple ID tokens  
3. **OTP Authentication** - SMS & Email with configurable rate limiting
4. **JWT Token Management** - Access & refresh tokens
5. **PostgreSQL Database** - EF Core with proper migrations
6. **API Endpoint Grouping** - All auth endpoints under `/api/auth`
7. **Rate Limiting** - Configurable OTP attempts (default: 5 per hour)

## 🚀 Quick Start

### 1. Install EF Core Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

### 2. Configure Database

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=realin_db;Username=YOUR_USER;Password=YOUR_PASSWORD"
  }
}
```

### 3. Create Database

```bash
# Option A: Using createdb command (if you have PostgreSQL CLI)
createdb -U postgres realin_db

# Option B: Using psql
psql -U postgres
CREATE DATABASE realin_db;
\q
```

### 4. Run Migrations

```bash
cd RealinServer/RealinApi
dotnet ef database update
```

### 5. Configure OAuth & JWT

Edit `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "YOUR_STRONG_SECRET_KEY_AT_LEAST_32_CHARS"
  },
  "OAuth": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID"
    },
    "Apple": {
      "ClientId": "YOUR_APPLE_CLIENT_ID"
    }
  }
}
```

### 6. Run the API

```bash
dotnet run
```

API runs at: **https://localhost:5001**  
Swagger UI: **https://localhost:5001/swagger**

## 📝 Testing

### Test OTP Flow (Development)

1. **Request OTP via Email:**
```bash
curl -X POST https://localhost:5001/api/auth/otp/request \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","method":"email"}'
```

2. **Check console logs for OTP code** (in development, OTP is logged)

3. **Verify OTP:**
```bash
curl -X POST https://localhost:5001/api/auth/otp/verify \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","code":"123456"}'
```

4. **Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64token...",
  "expiresAt": "2024-12-01T12:00:00Z",
  "user": {
    "id": "uuid",
    "email": "test@example.com",
    "provider": "Otp"
  }
}
```

### Test with Swagger

1. Go to https://localhost:5001/swagger
2. Try `/api/auth/otp/request` endpoint
3. Check console logs for OTP
4. Try `/api/auth/otp/verify` with the code
5. Use returned `accessToken` for authenticated requests

## 📊 Database Schema

**Users Table:**
- Stores all users (Google/Apple/OTP)
- One user can only have ONE auth provider

**RefreshTokens Table:**
- Manages refresh tokens
- 30-day expiration
- Device tracking

**OtpSessions Table:**
- Stores OTP codes with expiration
- Tracks verification attempts
- Rate limiting enforcement

## 🔒 Security Features

✅ JWT token validation  
✅ Refresh token rotation  
✅ OTP rate limiting (5 attempts per hour by default)  
✅ OTP expiration (10 minutes by default)  
✅ Provider-based user segregation (one provider per user)  

## ⚙️ Configuration Reference

### OTP Settings

```json
{
  "Otp": {
    "ExpirationMinutes": "10",        // How long OTP is valid
    "MaxAttemptsPerWindow": "5",      // Max OTP requests allowed
    "RateLimitWindowMinutes": "60",   // Time window for rate limit
    "Length": "6"                     // OTP code length
  }
}
```

### JWT Settings

```json
{
  "Jwt": {
    "SecretKey": "min_32_chars",
    "Issuer": "RealinApi",
    "Audience": "RealinApp",
    "AccessTokenExpirationMinutes": "60"
  }
}
```

## 📁 Project Structure

```
RealinApi/
├── Data/
│   ├── AppDbContext.cs              # EF Core DbContext
│   ├── AppDbContextFactory.cs       # Design-time factory
│   └── Entities/                    # Database entities
├── Features/
│   └── Auth/
│       ├── AuthEndpoints.cs         # Route mappings
│       ├── AuthService.cs           # Business logic
│       └── Models/                  # Request/Response DTOs
├── Infrastructure/
│   ├── Authentication/
│   │   ├── JwtService.cs           # JWT generation/validation
│   │   └── OtpService.cs           # OTP logic & rate limiting
│   ├── ExternalServices/
│   │   ├── GoogleAuthService.cs    # Google token validation
│   │   └── AppleAuthService.cs     # Apple token validation
│   └── Messaging/
│       ├── SmsService.cs           # SMS sending (stub)
│       └── EmailService.cs         # Email sending (stub)
├── Common/                          # Shared utilities
├── Migrations/                      # EF Core migrations
└── Program.cs                       # App entry point
```

## 🔧 Next Steps for Production

- [ ] Implement real SMS provider (Twilio, AWS SNS)
- [ ] Implement real Email provider (SendGrid, AWS SES)
- [ ] Complete Apple token validation with public key fetching
- [ ] Add HTTPS certificate for production
- [ ] Configure production CORS policy
- [ ] Set up logging (Serilog, Application Insights)
- [ ] Add health checks for database
- [ ] Implement user profile endpoints
- [ ] Add token revocation endpoint
- [ ] Set up CI/CD pipeline

## 🐛 Troubleshooting

**Migration fails:**
```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Build errors:**
```bash
dotnet clean
dotnet restore
dotnet build
```

**Database connection fails:**
- Check PostgreSQL is running: `pg_isready`
- Verify connection string in `appsettings.json`
- Test connection: `psql -U postgres -d realin_db`

## 📚 Resources

- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Google Sign-In](https://developers.google.com/identity/sign-in/web)
- [Sign in with Apple](https://developer.apple.com/sign-in-with-apple/)

---

**Built with .NET 10.0 Minimal APIs** 🚀
