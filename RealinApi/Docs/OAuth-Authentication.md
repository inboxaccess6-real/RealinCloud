# OAuth Authentication Implementation

## Overview

This document describes the OAuth 2.0 authentication implementation for the RealinApi using Swift Hummingbird and FluentKit with PostgreSQL. The system supports both **Google Sign-In** and **Apple Sign-In** as OAuth providers.

## Architecture

### Flow Diagram

```
Frontend (Mobile/Web)
    ↓
1. User initiates Google/Apple Sign-In
    ↓
2. OAuth Provider (Google/Apple) returns ID token
    ↓
3. Frontend sends ID token to Backend
    ↓
4. Backend verifies token with OAuth Provider
    ↓
5. Backend creates/updates user in database
    ↓
6. Backend generates JWT token
    ↓
7. Frontend receives JWT token
    ↓
8. Frontend includes JWT in subsequent requests (Authorization: Bearer <token>)
```

### Components

1. **Models**: User entity with Fluent ORM
2. **Services**: OAuth verification and JWT generation
3. **Controllers**: Authentication endpoints
4. **Middleware**: JWT verification for protected routes
5. **Migrations**: Database schema management

## Implementation Details

### 1. User Model

**File**: `Sources/App/Models/User.swift`

The `User` model represents an authenticated user with the following fields:

- `id`: UUID (Primary Key)
- `oauthProvider`: Enum (google/apple)
- `oauthId`: String (Unique ID from OAuth provider)
- `email`: String (User's email address)
- `name`: String? (Optional full name)
- `pictureUrl`: String? (Optional profile picture URL)
- `createdAt`: Date (Auto-generated)
- `updatedAt`: Date (Auto-updated)

**Unique Constraints**:
- Combination of `oauth_provider` + `oauth_id`
- `email` (unique across all users)

### 2. Database Migration

**File**: `Sources/App/Migrations/CreateUser.swift`

Auto-migration is configured in `App+build.swift`:

```swift
fluent.migrations.add(CreateUser())
try await fluent.migrate()
```

The migration creates the `users` table with appropriate indexes and constraints.

### 3. OAuth Services

#### Google OAuth Service

**File**: `Sources/App/Services/GoogleOAuthService.swift`

**Endpoint**: `https://oauth2.googleapis.com/tokeninfo`

**Process**:
1. Receives ID token from frontend
2. Sends token to Google's tokeninfo endpoint
3. Validates response and email verification status
4. Returns user information (sub, email, name, picture)

**Response Structure**:
```json
{
  "sub": "1234567890",
  "email": "user@example.com",
  "email_verified": "true",
  "name": "John Doe",
  "picture": "https://..."
}
```

#### Apple OAuth Service

**File**: `Sources/App/Services/AppleOAuthService.swift`

**Endpoint**: `https://appleid.apple.com/auth/keys`

**Process**:
1. Receives ID token (JWT) from frontend
2. Fetches Apple's public keys
3. Verifies JWT signature using RSA-256
4. Validates claims (issuer, audience, expiration)
5. Returns user information (sub, email)

**JWT Claims**:
```json
{
  "iss": "https://appleid.apple.com",
  "sub": "unique-user-id",
  "aud": "com.your.bundle.id",
  "exp": 1234567890,
  "email": "user@privaterelay.appleid.com"
}
```

**Note**: Apple provides user name only on the first sign-in attempt.

### 4. JWT Service

**File**: `Sources/App/Services/JWTService.swift`

**Purpose**: Generate and verify JWT tokens for authenticated sessions.

**Token Payload**:
```json
{
  "userId": "uuid-here",
  "email": "user@example.com",
  "exp": 1234567890,
  "iat": 1234567890
}
```

**Configuration**:
- Algorithm: HMAC SHA-256
- Expiration: 30 days (configurable)
- Secret: Set via `JWT_SECRET` environment variable

### 5. Authentication Controller

**File**: `Sources/App/Contorllers/AuthController.swift`

Provides three endpoints:

#### POST `/auth/google/login`
Login with Google ID token.

**Request**:
```json
{
  "idToken": "google-id-token-here"
}
```

**Response** (200 OK):
```json
{
  "token": "jwt-token-here",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "name": "John Doe",
    "pictureUrl": "https://...",
    "oauthProvider": "google",
    "createdAt": "2025-11-15T00:00:00Z"
  }
}
```

#### POST `/auth/apple/login`
Login with Apple ID token.

**Request**:
```json
{
  "idToken": "apple-id-token-here",
  "name": "John Doe"
}
```

**Note**: `name` field is optional and should be sent only on first sign-in.

**Response** (200 OK):
```json
{
  "token": "jwt-token-here",
  "user": {
    "id": "uuid",
    "email": "user@privaterelay.appleid.com",
    "name": "John Doe",
    "oauthProvider": "apple",
    "createdAt": "2025-11-15T00:00:00Z"
  }
}
```

#### GET `/auth/me`
Get current authenticated user (requires JWT token).

**Request Headers**:
```
Authorization: Bearer <jwt-token>
```

**Response** (200 OK):
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "name": "John Doe",
  "pictureUrl": "https://...",
  "oauthProvider": "google",
  "createdAt": "2025-11-15T00:00:00Z"
}
```

### 6. Authentication Middleware

**File**: `Sources/App/Middleware/AuthenticationMiddleware.swift`

**Purpose**: Protect routes by verifying JWT tokens.

**Usage** (example for future protected routes):
```swift
router.group()
    .add(middleware: JWTAuthenticationMiddleware(jwtService: jwtService))
    .get("/protected-route") { request, context in
        // Access user ID from context
        guard let userId = context.userId else {
            throw Abort(.unauthorized)
        }
        // Your protected route logic
    }
```

## Configuration

### Environment Variables

Create a `.env` file or set environment variables:

```bash
# Database Configuration
DATABASE_URL=postgres://username:password@localhost:5432/realin_db

# JWT Secret (CHANGE IN PRODUCTION!)
JWT_SECRET=your-secure-random-secret-key-here

# Apple Bundle ID
APPLE_BUNDLE_ID=com.example.realin

# Optional: Log Level
LOG_LEVEL=info
```

### Command Line Arguments

Alternatively, pass arguments when running:

```bash
swift run App \
  --database-url postgres://localhost/realin_db \
  --jwt-secret your-secret \
  --apple-bundle-id com.example.realin \
  --hostname 0.0.0.0 \
  --port 8080
```

## Database Setup

### 1. Install PostgreSQL

**macOS** (using Homebrew):
```bash
brew install postgresql@16
brew services start postgresql@16
```

**Linux**:
```bash
sudo apt-get install postgresql
sudo systemctl start postgresql
```

### 2. Create Database

```bash
# Connect to PostgreSQL
psql postgres

# Create database
CREATE DATABASE realin_db;

# Create user (optional)
CREATE USER realin_user WITH PASSWORD 'your-password';
GRANT ALL PRIVILEGES ON DATABASE realin_db TO realin_user;

# Exit
\q
```

### 3. Auto-Migration

The application automatically runs migrations on startup. The `CreateUser` migration will:
- Create the `users` table
- Add indexes and unique constraints
- Set up timestamps

## Security Considerations

### 1. JWT Secret

**CRITICAL**: Change the default JWT secret in production!

Generate a secure secret:
```bash
openssl rand -base64 32
```

Set it as an environment variable:
```bash
export JWT_SECRET="your-generated-secret"
```

### 2. HTTPS/TLS

**Production**: Always use HTTPS for your API to prevent token interception.

Configure TLS in Hummingbird:
```swift
// Add TLS configuration to Application
```

### 3. Database Connection

**Production**: Use TLS for PostgreSQL connections:

```swift
PostgresConfiguration(
    hostname: host,
    port: port,
    username: username,
    password: password,
    database: database,
    tls: .require // Enable TLS
)
```

### 4. Token Expiration

Current token expiration: **30 days**

Adjust in `JWTService.swift`:
```swift
init(secret: String, tokenExpirationTime: TimeInterval = 7 * 24 * 60 * 60) // 7 days
```

### 5. Rate Limiting

**TODO**: Add rate limiting middleware to prevent brute force attacks on authentication endpoints.

### 6. CORS Configuration

If serving a web frontend, configure CORS:

```swift
router.addMiddleware {
    CORSMiddleware(
        allowedOrigins: ["https://your-frontend.com"],
        allowedMethods: [.GET, .POST],
        allowedHeaders: [.authorization, .contentType]
    )
}
```

## Testing

### 1. Health Check

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

### 2. Google Login

```bash
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "your-google-token"}'
```

### 3. Apple Login

```bash
curl -X POST http://localhost:8080/auth/apple/login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "your-apple-token", "name": "John Doe"}'
```

### 4. Get Current User

```bash
curl http://localhost:8080/auth/me \
  -H "Authorization: Bearer your-jwt-token"
```

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "error": "Invalid request format"
}
```

#### 401 Unauthorized
```json
{
  "error": "Invalid or expired token"
}
```

#### 404 Not Found
```json
{
  "error": "User not found"
}
```

#### 500 Internal Server Error
```json
{
  "error": "Database not available"
}
```

## Troubleshooting

### Database Connection Issues

**Error**: "Connection refused"
- Ensure PostgreSQL is running: `brew services list`
- Check connection string in `DATABASE_URL`

### Google Token Verification Fails

**Error**: "Invalid Google token"
- Verify token is fresh (tokens expire quickly)
- Check network connectivity to Google's servers
- Ensure token was generated for the correct client ID

### Apple Token Verification Fails

**Error**: "Token audience does not match"
- Verify `APPLE_BUNDLE_ID` matches your app's bundle ID
- Check that the token was generated for your app

### Migration Errors

**Error**: "Table already exists"
- The migration system tracks completed migrations
- To reset: Drop the database and recreate it
- Or manually delete migration records from `_fluent_migrations` table

## Frontend Integration

### Google Sign-In (React Native Example)

```javascript
import { GoogleSignin } from '@react-native-google-signin/google-signin';

// Configure Google Sign-In
GoogleSignin.configure({
  webClientId: 'your-web-client-id.apps.googleusercontent.com',
});

// Sign in
const signInWithGoogle = async () => {
  await GoogleSignin.hasPlayServices();
  const userInfo = await GoogleSignin.signIn();
  const idToken = userInfo.idToken;
  
  // Send to backend
  const response = await fetch('http://your-api.com/auth/google/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken })
  });
  
  const data = await response.json();
  // Store data.token for subsequent requests
};
```

### Apple Sign-In (React Native Example)

```javascript
import appleAuth from '@invertase/react-native-apple-authentication';

const signInWithApple = async () => {
  const appleAuthRequestResponse = await appleAuth.performRequest({
    requestedOperation: appleAuth.Operation.LOGIN,
    requestedScopes: [appleAuth.Scope.EMAIL, appleAuth.Scope.FULL_NAME],
  });
  
  const { identityToken, fullName } = appleAuthRequestResponse;
  const name = fullName ? `${fullName.givenName} ${fullName.familyName}` : null;
  
  // Send to backend
  const response = await fetch('http://your-api.com/auth/apple/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken: identityToken, name })
  });
  
  const data = await response.json();
  // Store data.token for subsequent requests
};
```

### Making Authenticated Requests

```javascript
const fetchUserProfile = async (jwtToken) => {
  const response = await fetch('http://your-api.com/auth/me', {
    headers: {
      'Authorization': `Bearer ${jwtToken}`
    }
  });
  
  const user = await response.json();
  return user;
};
```

## Future Enhancements

### 1. Refresh Tokens
Implement refresh token mechanism for longer sessions without re-authentication.

### 2. User Logout
Add endpoint to invalidate tokens (requires token blacklist or database storage).

### 3. Email Verification
Send verification emails for additional security.

### 4. Two-Factor Authentication
Add optional 2FA for enhanced security.

### 5. User Profile Updates
Add endpoints to update user profile information.

### 6. Social Profile Sync
Periodically sync profile information from OAuth providers.

## License

This implementation is part of the RealinApi project.

## Support

For issues or questions, please contact your development team.
