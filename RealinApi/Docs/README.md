# Documentation Index

Welcome to the RealinApi documentation! This folder contains comprehensive guides for setting up and using the authentication system with OAuth and OTP support.

---

## 📚 Documentation Files

### 1. [Quick Start Guide](./Quick-Start.md)
**Start here!** Get up and running in minutes with step-by-step instructions.

**Contents**:
- Installation prerequisites
- Database setup
- Environment configuration
- Running the application
- Basic testing
- Common issues and solutions

**Perfect for**: First-time setup, new developers joining the project

---

### 2. [OAuth Authentication Guide](./OAuth-Authentication.md)
Complete implementation details for OAuth 2.0 with Google and Apple.

**Contents**:
- Architecture overview and flow diagrams
- Component descriptions (Models, Services, Controllers)
- Google OAuth implementation
- Apple Sign-In implementation
- JWT token management
- Security best practices
- Frontend integration examples
- Testing strategies

**Perfect for**: Understanding how OAuth works, implementing frontend integration

---

### 3. [OTP Authentication Guide](./OTP-Authentication-Guide.md)
**NEW!** Passwordless email authentication using One-Time Passwords.

**Contents**:
- OTP authentication overview
- API endpoints (send/verify OTP)
- Authentication flow diagrams
- Rate limiting and security features
- Frontend integration examples (React, Swift)
- Email service configuration
- Testing and troubleshooting
- Best practices

**Perfect for**: Implementing email-based passwordless login, understanding OTP flow

---

### 4. [API Endpoints Reference](./API-Endpoints.md)
Detailed API documentation for all available endpoints.

**Contents**:
- Endpoint specifications
- Request/response formats
- Authentication requirements
- Error codes and handling
- cURL examples
- Postman collection setup
- Best practices for API usage

**Perfect for**: Frontend developers, API consumers, testing

---

### 5. [Refresh Token Guide](./Refresh-Token-Guide.md)
Session management with access and refresh tokens.

**Contents**:
- Dual-token authentication strategy
- Token expiration and refresh flow
- Security considerations
- Frontend implementation
- Token revocation
- Best practices

**Perfect for**: Understanding session management, implementing token refresh

---

### 6. [Database Schema](./Database-Schema.md)
PostgreSQL database structure and Fluent ORM mapping.

**Contents**:
- Table schemas
- Indexes and constraints
- Fluent model mapping
- Common queries
- Migration management
- Performance optimization
- Backup and maintenance

**Perfect for**: Database administrators, backend developers, data modeling

---

## 🚀 Quick Navigation

### For New Developers
1. Read [Quick Start Guide](./Quick-Start.md) to set up your environment
2. Review [OAuth Authentication Guide](./OAuth-Authentication.md) to understand OAuth
3. Review [OTP Authentication Guide](./OTP-Authentication-Guide.md) to understand email login
4. Reference [API Endpoints](./API-Endpoints.md) when building features

### For Frontend Developers
1. Check [API Endpoints Reference](./API-Endpoints.md) for endpoint details
2. Review OAuth flow in [OAuth Authentication Guide](./OAuth-Authentication.md)
3. Review OTP flow in [OTP Authentication Guide](./OTP-Authentication-Guide.md)
4. Check [Refresh Token Guide](./Refresh-Token-Guide.md) for session management
5. Use the integration examples for your platform

### For Database Work
1. Start with [Database Schema](./Database-Schema.md)
2. Reference [Quick Start Guide](./Quick-Start.md) for setup
3. Review migration examples in [OAuth Authentication Guide](./OAuth-Authentication.md)

---

## 📖 Overview

The RealinApi is a Swift Hummingbird-based REST API that provides OAuth 2.0 authentication using:

- **Google Sign-In**: Verify Google ID tokens
- **Apple Sign-In**: Verify Apple ID tokens
- **JWT Authentication**: Secure session management
- **PostgreSQL**: User data storage with Fluent ORM
- **Auto-migrations**: Automatic database schema management

---

## 🔑 Key Features

### OAuth Providers
- ✅ Google OAuth 2.0
- ✅ Apple Sign In
- 🔄 Future: Facebook, Twitter, GitHub, etc.

### Security
- ✅ JWT token-based authentication
- ✅ HMAC-SHA256 token signing
- ✅ Token expiration handling
- ✅ Secure password-less authentication

### Database
- ✅ PostgreSQL with Fluent ORM
- ✅ Automatic migrations
- ✅ Unique constraints
- ✅ Indexed queries

### API Design
- ✅ RESTful endpoints
- ✅ JSON request/response
- ✅ Comprehensive error handling
- ✅ Health check endpoint

---

## 🏗️ Architecture

```
┌─────────────────┐
│   Frontend      │
│ (iOS/Android/   │
│  Web)           │
└────────┬────────┘
         │ ID Token
         ▼
┌─────────────────┐
│   RealinApi     │
│  (Hummingbird)  │
├─────────────────┤
│ • OAuth Services│
│ • JWT Service   │
│ • Controllers   │
│ • Middleware    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   PostgreSQL    │
│   (Database)    │
└─────────────────┘
```

---

## 📋 Prerequisites

- macOS 14.0+ or Linux
- Swift 6.0+
- PostgreSQL 14+
- Xcode 15.0+ (optional)

---

## 🛠️ Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | Hummingbird 2.0 |
| ORM | Fluent (HummingbirdFluent) |
| Database | PostgreSQL |
| JWT | JWTKit |
| HTTP Client | AsyncHTTPClient |
| Language | Swift 6.0 |

---

## 📝 API Overview

### Public Endpoints
- `GET /` - Root endpoint
- `GET /health` - Health check
- `POST /auth/google/login` - Google OAuth login
- `POST /auth/apple/login` - Apple OAuth login

### Protected Endpoints
- `GET /auth/me` - Get current user (requires JWT)

---

## 🔐 Authentication Flow

1. **Frontend**: User initiates OAuth sign-in (Google/Apple)
2. **OAuth Provider**: Returns ID token to frontend
3. **Frontend**: Sends ID token to RealinApi
4. **RealinApi**: Verifies token with OAuth provider
5. **RealinApi**: Creates/updates user in database
6. **RealinApi**: Generates JWT token
7. **Frontend**: Receives JWT token
8. **Frontend**: Includes JWT in future requests

---

## 🌐 Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `DATABASE_URL` | PostgreSQL connection string | `postgres://localhost/realin_db` |
| `JWT_SECRET` | Secret key for JWT signing | `your-secret-key` |
| `APPLE_BUNDLE_ID` | Apple app bundle ID | `com.example.realin` |
| `LOG_LEVEL` | Logging verbosity | `info`, `debug` |

---

## 📁 Project Structure

```
RealinApi/
├── Sources/
│   └── App/
│       ├── App.swift              # Entry point
│       ├── App+build.swift        # App configuration
│       ├── Controllers/           # API endpoints
│       │   └── AuthController.swift
│       ├── Models/                # Database models
│       │   └── User.swift
│       ├── Services/              # Business logic
│       │   ├── GoogleOAuthService.swift
│       │   ├── AppleOAuthService.swift
│       │   └── JWTService.swift
│       ├── Middleware/            # Request middleware
│       │   └── AuthenticationMiddleware.swift
│       └── Migrations/            # Database migrations
│           └── CreateUser.swift
├── Docs/                          # 📚 You are here!
├── Package.swift                  # Dependencies
└── README.md                      # Project README
```

---

## 🧪 Testing

### Manual Testing with cURL

**Health Check**:
```bash
curl http://localhost:8080/health
```

**Google Login**:
```bash
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "your-token"}'
```

**Get Current User**:
```bash
curl http://localhost:8080/auth/me \
  -H "Authorization: Bearer your-jwt-token"
```

### Automated Testing

See [Quick Start Guide](./Quick-Start.md) for unit test setup.

---

## 🚨 Common Issues

| Issue | Solution |
|-------|----------|
| Port already in use | Change port with `--port 8081` |
| Database connection failed | Ensure PostgreSQL is running |
| Invalid Google token | Token may be expired or invalid |
| Migration errors | Check if migrations already ran |

See [Quick Start Guide](./Quick-Start.md#common-issues--solutions) for detailed troubleshooting.

---

## 🔒 Security Checklist

- [ ] Change default JWT secret
- [ ] Enable HTTPS/TLS in production
- [ ] Use TLS for database connections
- [ ] Implement rate limiting
- [ ] Configure CORS properly
- [ ] Set up monitoring and alerts
- [ ] Regular security audits
- [ ] Keep dependencies updated

---

## 📦 Deployment

### Development
```bash
swift run App
```

### Production
```bash
swift build -c release
.build/release/App \
  --hostname 0.0.0.0 \
  --port 8080
```

### Docker
```bash
docker build -t realinapi .
docker run -p 8080:8080 realinapi
```

See [Quick Start Guide](./Quick-Start.md#production-deployment) for detailed deployment instructions.

---

## 🤝 Contributing

1. Read all documentation
2. Follow Swift conventions
3. Write tests for new features
4. Update documentation
5. Submit pull request

---

## 📞 Support

- **Documentation**: This folder contains all guides
- **Issues**: Create an issue in the repository
- **Questions**: Contact the development team

---

## 📜 License

[Your License Here]

---

## 🗺️ Roadmap

### Current Features
- ✅ Google OAuth
- ✅ Apple Sign In
- ✅ JWT authentication
- ✅ PostgreSQL storage
- ✅ Auto-migrations

### Planned Features
- 🔄 Refresh tokens
- 🔄 User profile updates
- 🔄 Email verification
- 🔄 Two-factor authentication
- 🔄 Rate limiting
- 🔄 CORS middleware
- 🔄 Additional OAuth providers

---

## 📚 Additional Resources

### External Documentation
- [Hummingbird Documentation](https://docs.hummingbird.codes/)
- [Fluent Documentation](https://docs.vapor.codes/fluent/overview/)
- [JWTKit Documentation](https://github.com/vapor/jwt-kit)
- [Google Sign-In](https://developers.google.com/identity/sign-in/web)
- [Apple Sign In](https://developer.apple.com/sign-in-with-apple/)

### Tutorials
- Swift Server Working Group: https://www.swift.org/sswg/
- Swift on Server: https://swiftonserver.com/

---

**Last Updated**: November 15, 2025  
**Version**: 1.0.0
