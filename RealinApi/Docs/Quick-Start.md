# Quick Start Guide

This guide will help you get the RealinApi up and running with OAuth authentication in just a few minutes.

---

## Prerequisites

Before you begin, ensure you have the following installed:

- **macOS**: 14.0 or later
- **Swift**: 6.0 or later
- **Xcode**: 15.0 or later (optional, for IDE support)
- **PostgreSQL**: 14+ (16 recommended)
- **Git**: For cloning the repository

### Install PostgreSQL

**macOS** (using Homebrew):
```bash
brew install postgresql@16
brew services start postgresql@16
```

**Linux** (Ubuntu/Debian):
```bash
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib
sudo systemctl start postgresql
```

---

## Step 1: Clone the Repository

```bash
git clone <your-repo-url>
cd RealinApi
```

---

## Step 2: Setup Database

### Create Database

```bash
# Connect to PostgreSQL
psql postgres

# In psql prompt:
CREATE DATABASE realin_db;

# Optional: Create dedicated user
CREATE USER realin_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE realin_db TO realin_user;

# Exit
\q
```

### Verify Connection

```bash
psql realin_db -c "SELECT version();"
```

---

## Step 3: Configure Environment

Create a `.env` file in the project root:

```bash
cat > .env << 'EOF'
# Database Configuration
DATABASE_URL=postgres://localhost/realin_db

# JWT Secret (IMPORTANT: Change this in production!)
JWT_SECRET=your-super-secret-key-change-this-in-production

# Apple Bundle ID (replace with your actual bundle ID)
APPLE_BUNDLE_ID=com.example.realin

# Optional: Log Level
LOG_LEVEL=debug
EOF
```

**Security Note**: Generate a secure JWT secret:
```bash
openssl rand -base64 32
```

---

## Step 4: Install Dependencies

The project uses Swift Package Manager (SPM). Dependencies will be resolved automatically:

```bash
swift package resolve
```

Or in Xcode:
1. Open `RealinApi.xcodeproj`
2. Wait for package resolution to complete

---

## Step 5: Build the Project

### Using Swift CLI

```bash
swift build
```

### Using Xcode

1. Open `RealinApi.xcodeproj`
2. Select the "App" scheme
3. Press `Cmd + B` to build

---

## Step 6: Run the Application

### Using Swift CLI

```bash
swift run App
```

### Using Xcode

1. Select the "App" scheme
2. Press `Cmd + R` to run

### With Custom Configuration

```bash
swift run App \
  --hostname 0.0.0.0 \
  --port 8080 \
  --database-url postgres://localhost/realin_db \
  --jwt-secret your-secret-key \
  --apple-bundle-id com.example.realin
```

---

## Step 7: Verify Installation

### Check Health Endpoint

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

### Check Database Migration

The application automatically runs migrations on startup. Check the logs for:

```
[ INFO ] Database migrations completed successfully
```

### Verify Database Tables

```bash
psql realin_db -c "\dt"
```

You should see:
```
            List of relations
 Schema |        Name         | Type  |  Owner   
--------+---------------------+-------+----------
 public | _fluent_migrations  | table | postgres
 public | users               | table | postgres
```

---

## Step 8: Test OAuth Endpoints

### Google OAuth (Mock Test)

**Note**: For real testing, you'll need a valid Google ID token from your frontend.

```bash
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "your-google-id-token-here"
  }'
```

### Get Current User

After successful login, use the returned JWT token:

```bash
curl http://localhost:8080/auth/me \
  -H "Authorization: Bearer your-jwt-token-here"
```

---

## Frontend Integration

### Google Sign-In Setup

1. **Create Google OAuth Client**:
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select existing
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Note your Client ID

2. **Configure Frontend** (React Native example):

```bash
npm install @react-native-google-signin/google-signin
```

```javascript
import { GoogleSignin } from '@react-native-google-signin/google-signin';

GoogleSignin.configure({
  webClientId: 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com',
});

async function signInWithGoogle() {
  await GoogleSignin.hasPlayServices();
  const userInfo = await GoogleSignin.signIn();
  
  const response = await fetch('http://localhost:8080/auth/google/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken: userInfo.idToken })
  });
  
  const data = await response.json();
  // Store data.token for future requests
}
```

### Apple Sign-In Setup

1. **Configure Apple Developer Account**:
   - Add "Sign in with Apple" capability to your app
   - Configure your app's bundle ID
   - Enable in Apple Developer Portal

2. **Configure Frontend** (React Native example):

```bash
npm install @invertase/react-native-apple-authentication
```

```javascript
import appleAuth from '@invertase/react-native-apple-authentication';

async function signInWithApple() {
  const appleAuthRequestResponse = await appleAuth.performRequest({
    requestedOperation: appleAuth.Operation.LOGIN,
    requestedScopes: [appleAuth.Scope.EMAIL, appleAuth.Scope.FULL_NAME],
  });
  
  const { identityToken, fullName } = appleAuthRequestResponse;
  const name = fullName ? `${fullName.givenName} ${fullName.familyName}` : null;
  
  const response = await fetch('http://localhost:8080/auth/apple/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ 
      idToken: identityToken,
      name: name
    })
  });
  
  const data = await response.json();
  // Store data.token for future requests
}
```

---

## Common Issues & Solutions

### Issue 1: Port Already in Use

**Error**: "Address already in use"

**Solution**: Change the port or kill the process:
```bash
lsof -ti:8080 | xargs kill -9
# Or use a different port
swift run App --port 8081
```

### Issue 2: Database Connection Failed

**Error**: "Connection refused" or "could not connect to server"

**Solution**:
```bash
# Check if PostgreSQL is running
brew services list

# Start PostgreSQL
brew services start postgresql@16

# Verify connection
psql postgres -c "SELECT 1"
```

### Issue 3: Migration Errors

**Error**: "relation already exists"

**Solution**: Migrations already applied. To reset:
```bash
psql realin_db -c "DROP TABLE IF EXISTS users CASCADE;"
psql realin_db -c "DROP TABLE IF EXISTS _fluent_migrations CASCADE;"
# Restart the app to re-run migrations
```

### Issue 4: Swift Build Errors

**Error**: Package resolution issues

**Solution**:
```bash
# Clean build artifacts
rm -rf .build
swift package clean
swift package resolve
swift build
```

### Issue 5: Invalid Google Token

**Error**: "Invalid Google token"

**Solution**:
- Ensure the token is fresh (they expire quickly)
- Verify you're using the correct Google Client ID in frontend
- Check network connectivity to Google's servers

---

## Development Tips

### Hot Reload

For faster development, use `swift run` with file watching:

```bash
# Install fswatch (macOS)
brew install fswatch

# Watch for changes and rebuild
fswatch -o Sources | xargs -n1 -I{} swift run App
```

### Database GUI

Use a PostgreSQL GUI for easier database management:

**macOS**:
- [Postico](https://eggerapps.at/postico/) (recommended)
- [TablePlus](https://tableplus.com/)
- [pgAdmin](https://www.pgadmin.org/)

**Connection Details**:
- Host: localhost
- Port: 5432
- Database: realin_db
- User: postgres (or your custom user)

### Xcode Setup

1. Open `RealinApi.xcodeproj`
2. Select "App" scheme
3. Edit scheme → Run → Arguments
4. Add environment variables:
   - `DATABASE_URL`: `postgres://localhost/realin_db`
   - `JWT_SECRET`: `your-secret`
   - `APPLE_BUNDLE_ID`: `com.example.realin`

### Debug Logging

Set log level to debug for verbose output:

```bash
swift run App --log-level debug
```

Or in code (`App+build.swift`):
```swift
logger.logLevel = .debug
```

---

## Production Deployment

### Environment Variables

Set these in your production environment:

```bash
export DATABASE_URL="postgres://user:pass@host:5432/db?sslmode=require"
export JWT_SECRET="$(openssl rand -base64 32)"
export APPLE_BUNDLE_ID="com.yourapp.bundle"
export LOG_LEVEL="info"
```

### TLS/SSL Configuration

Enable TLS for database connections:

```swift
// In App+build.swift
PostgresConfiguration(
    hostname: host,
    port: port,
    username: username,
    password: password,
    database: database,
    tls: .require // Enable TLS
)
```

### Docker Deployment

Create a `Dockerfile`:

```dockerfile
FROM swift:6.0

WORKDIR /app

COPY Package.swift Package.resolved ./
RUN swift package resolve

COPY . .
RUN swift build -c release

EXPOSE 8080

CMD [".build/release/App"]
```

Build and run:
```bash
docker build -t realinapi .
docker run -p 8080:8080 \
  -e DATABASE_URL="postgres://..." \
  -e JWT_SECRET="..." \
  realinapi
```

### Health Checks

Configure health check endpoint for monitoring:

```bash
curl http://your-server.com/health
```

---

## Next Steps

1. **Read the Documentation**:
   - [OAuth Authentication Guide](./OAuth-Authentication.md)
   - [API Endpoints Reference](./API-Endpoints.md)
   - [Database Schema](./Database-Schema.md)

2. **Implement Frontend**:
   - Set up Google/Apple Sign-In in your mobile app
   - Test OAuth flow end-to-end

3. **Add Features**:
   - User profile updates
   - Password reset (if adding email/password auth)
   - Two-factor authentication
   - Refresh tokens

4. **Security Hardening**:
   - Enable HTTPS/TLS
   - Add rate limiting
   - Implement CORS properly
   - Set up monitoring and logging

5. **Testing**:
   - Write unit tests
   - Add integration tests
   - Load testing with tools like `wrk` or `ab`

---

## Getting Help

- **Documentation**: Check the `Docs/` folder
- **Logs**: Review application logs for errors
- **Community**: Swift Forums, Hummingbird Discord
- **Issues**: Create an issue in your repository

---

## Summary

You now have a fully functional OAuth authentication API with:

✅ Google Sign-In support  
✅ Apple Sign-In support  
✅ PostgreSQL database with auto-migrations  
✅ JWT-based authentication  
✅ Protected endpoints  
✅ Comprehensive error handling  

Happy coding! 🚀
