# RealinApi

A modern, secure REST API built with Swift Hummingbird that provides OAuth 2.0 authentication for mobile and web applications.

## 🚀 Features

- ✅ **Google OAuth 2.0** - Authenticate users with Google Sign-In
- ✅ **Apple Sign In** - Authenticate users with Sign in with Apple
- ✅ **JWT Authentication** - Secure session management with JSON Web Tokens
- ✅ **PostgreSQL Database** - Robust data storage with Fluent ORM
- ✅ **Auto-migrations** - Automatic database schema management
- ✅ **Type-safe** - Built with Swift 6.0 for maximum safety
- ✅ **Async/Await** - Modern concurrency with Swift's async/await
- ✅ **Production-ready** - Comprehensive error handling and logging

## 📋 Prerequisites

- macOS 14.0+ or Linux
- Swift 6.0+
- PostgreSQL 14+
- Xcode 15.0+ (optional, for IDE support)

## 🏗️ Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | [Hummingbird 2.0](https://github.com/hummingbird-project/hummingbird) |
| ORM | [Fluent](https://github.com/vapor/fluent) (HummingbirdFluent) |
| Database | PostgreSQL |
| JWT | [JWTKit](https://github.com/vapor/jwt-kit) |
| HTTP Client | [AsyncHTTPClient](https://github.com/swift-server/async-http-client) |
| Language | Swift 6.0 |

## ⚡ Quick Start

### 1. Install PostgreSQL

**macOS**:
```bash
brew install postgresql@16
brew services start postgresql@16
```

### 2. Create Database

```bash
psql postgres -c "CREATE DATABASE realin_db;"
```

### 3. Configure Environment

Copy the example environment file and update values:

```bash
cp .env.example .env
```

Edit `.env`:
```bash
DATABASE_URL=postgres://localhost/realin_db
JWT_SECRET=$(openssl rand -base64 32)
APPLE_BUNDLE_ID=com.example.realin
```

### 4. Build and Run

```bash
swift run App
```

The API will start on `http://localhost:8080`

### 5. Verify Installation

```bash
curl http://localhost:8080/health
```

Expected response:
```json
{
  "status": "ok",
  "timestamp": "2025-11-15T10:30:00Z"
}
```

## 📚 Documentation

Comprehensive documentation is available in the `Docs/` folder:

- **[Quick Start Guide](./Docs/Quick-Start.md)** - Detailed setup instructions
- **[OAuth Authentication](./Docs/OAuth-Authentication.md)** - Complete OAuth implementation guide
- **[API Endpoints](./Docs/API-Endpoints.md)** - API reference and examples
- **[Database Schema](./Docs/Database-Schema.md)** - Database structure and queries

## 🔐 API Endpoints

### Public Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Root endpoint |
| GET | `/health` | Health check |
| POST | `/auth/google/login` | Login with Google |
| POST | `/auth/apple/login` | Login with Apple |

### Protected Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/auth/me` | Get current user | ✅ JWT Token |

## 🔑 Authentication Flow

```
1. Frontend initiates OAuth sign-in (Google/Apple)
2. OAuth provider returns ID token
3. Frontend sends ID token to /auth/{google|apple}/login
4. Backend verifies token with OAuth provider
5. Backend creates/updates user in database
6. Backend generates and returns JWT token
7. Frontend stores JWT token
8. Frontend includes JWT in Authorization header for protected routes
```

## 💻 Example Usage

### Google Login

```bash
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "your-google-id-token"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "email": "user@gmail.com",
    "name": "John Doe",
    "pictureUrl": "https://...",
    "oauthProvider": "google",
    "createdAt": "2025-11-15T10:30:00Z"
  }
}
```

### Get Current User

```bash
curl http://localhost:8080/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## 🎯 Frontend Integration

### React Native (Google Sign-In)

```javascript
import { GoogleSignin } from '@react-native-google-signin/google-signin';

async function signInWithGoogle() {
  const userInfo = await GoogleSignin.signIn();
  
  const response = await fetch('http://localhost:8080/auth/google/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken: userInfo.idToken })
  });
  
  const { token, user } = await response.json();
  // Store token for future requests
}
```

### React Native (Apple Sign-In)

```javascript
import appleAuth from '@invertase/react-native-apple-authentication';

async function signInWithApple() {
  const result = await appleAuth.performRequest({
    requestedOperation: appleAuth.Operation.LOGIN,
    requestedScopes: [appleAuth.Scope.EMAIL, appleAuth.Scope.FULL_NAME],
  });
  
  const response = await fetch('http://localhost:8080/auth/apple/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ 
      idToken: result.identityToken,
      name: `${result.fullName?.givenName} ${result.fullName?.familyName}`
    })
  });
  
  const { token, user } = await response.json();
  // Store token for future requests
}
```

## 📁 Project Structure

```
RealinApi/
├── Sources/
│   └── App/
│       ├── App.swift              # Application entry point
│       ├── App+build.swift        # App configuration & routing
│       ├── Controllers/           # API endpoint handlers
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
├── Docs/                          # 📚 Documentation
│   ├── README.md
│   ├── Quick-Start.md
│   ├── OAuth-Authentication.md
│   ├── API-Endpoints.md
│   └── Database-Schema.md
├── Package.swift                  # Swift Package Manager config
├── .env.example                   # Environment variables template
└── README.md                      # This file
```

## 🔒 Security

### Environment Variables

**Never commit sensitive data!** Always use environment variables:

```bash
# Generate a secure JWT secret
export JWT_SECRET=$(openssl rand -base64 32)

# Use SSL for database in production
export DATABASE_URL="postgres://user:pass@host:5432/db?sslmode=require"
```

### Production Checklist

- [ ] Change default JWT secret
- [ ] Enable HTTPS/TLS
- [ ] Use TLS for database connections
- [ ] Implement rate limiting
- [ ] Configure CORS properly
- [ ] Set up monitoring and alerts
- [ ] Regular security audits

## 🧪 Testing

Run tests with:

```bash
swift test
```

Manual API testing:

```bash
# Health check
curl http://localhost:8080/health

# Test endpoints (requires valid OAuth tokens)
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "token-from-frontend"}'
```

## 🐳 Docker Support

Build and run with Docker:

```bash
docker build -t realinapi .
docker run -p 8080:8080 \
  -e DATABASE_URL="postgres://..." \
  -e JWT_SECRET="..." \
  realinapi
```

## 🚀 Deployment

### Production Environment

Set these environment variables:

```bash
export DATABASE_URL="postgres://user:pass@host:5432/db?sslmode=require"
export JWT_SECRET="your-secure-random-key"
export APPLE_BUNDLE_ID="com.yourapp.bundle"
export LOG_LEVEL="info"
```

Build for production:

```bash
swift build -c release
.build/release/App --hostname 0.0.0.0 --port 8080
```

## 📊 Database

### Schema

The database includes a `users` table with:
- OAuth provider and ID
- Email (unique)
- Name and profile picture
- Timestamps

Auto-migrations run on startup. See [Database Schema](./Docs/Database-Schema.md) for details.

## 🛠️ Development

### Running in Development Mode

```bash
# With debug logging
swift run App --log-level debug

# On a different port
swift run App --port 8081
```

### Xcode Setup

1. Open `RealinApi.xcodeproj`
2. Select "App" scheme
3. Edit Scheme → Run → Arguments → Environment Variables
4. Add your environment variables
5. Run with `Cmd + R`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write/update tests
5. Update documentation
6. Submit a pull request

## 📝 License

[Your License Here]

## 🆘 Support

- **Documentation**: See `Docs/` folder
- **Issues**: Create an issue in the repository
- **Questions**: Contact the development team

## 🗺️ Roadmap

### Current (v1.0)
- ✅ Google OAuth
- ✅ Apple Sign In
- ✅ JWT authentication
- ✅ PostgreSQL with Fluent
- ✅ Auto-migrations

### Planned
- 🔄 Refresh tokens
- 🔄 User profile management
- 🔄 Email verification
- 🔄 Two-factor authentication
- 🔄 Rate limiting middleware
- 🔄 Additional OAuth providers (Facebook, GitHub, etc.)

## 📖 Resources

- [Hummingbird Documentation](https://docs.hummingbird.codes/)
- [Fluent Documentation](https://docs.vapor.codes/fluent/overview/)
- [Swift.org](https://swift.org/)
- [Google Sign-In Documentation](https://developers.google.com/identity)
- [Apple Sign In Documentation](https://developer.apple.com/sign-in-with-apple/)

---

**Built with ❤️ using Swift and Hummingbird**
