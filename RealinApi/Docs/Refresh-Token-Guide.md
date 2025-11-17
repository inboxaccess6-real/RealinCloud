# Refresh Token Implementation Guide

## Overview

The RealinApi now supports refresh tokens for maintaining user sessions without requiring frequent re-authentication. This implementation uses a dual-token approach:

- **Access Token** (JWT): Short-lived (30 minutes) for API requests
- **Refresh Token**: Long-lived (90 days) for obtaining new access tokens

---

## Architecture

### Token Lifecycle

```
┌─────────────┐
│   Login     │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────┐
│ Generate Access Token (30m) │
│ Generate Refresh Token (90d)│
└──────┬──────────────────────┘
       │
       ▼
┌─────────────────────┐
│ Store in Database   │
│ - user_id           │
│ - token             │
│ - expires_at        │
│ - device_name       │
│ - ip_address        │
└──────┬──────────────┘
       │
       ▼
┌──────────────────────┐
│ Return Both Tokens   │
│ to Frontend          │
└──────────────────────┘
```

### Token Refresh Flow

```
Access Token Expired
       │
       ▼
┌────────────────────┐
│ POST /auth/refresh │
│ with refresh token │
└─────────┬──────────┘
          │
          ▼
┌──────────────────────┐
│ Verify Refresh Token │
│ - Check expiration   │
│ - Check revoked      │
└─────────┬────────────┘
          │
          ▼
┌──────────────────────────┐
│ Generate New Tokens      │
│ - New access token       │
│ - New refresh token      │
│ - Revoke old refresh     │
└─────────┬────────────────┘
          │
          ▼
┌──────────────────┐
│ Return New Tokens│
└──────────────────┘
```

---

## Database Schema

### refresh_tokens Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PRIMARY KEY | Unique token ID |
| user_id | UUID | FK → users(id), ON DELETE CASCADE | Associated user |
| token | VARCHAR | UNIQUE, NOT NULL | Refresh token string |
| expires_at | TIMESTAMP | NOT NULL | Expiration date |
| is_revoked | BOOLEAN | NOT NULL | Revocation status |
| device_name | VARCHAR | NULL | Device identifier |
| ip_address | VARCHAR | NULL | Client IP address |
| created_at | TIMESTAMP | NULL | Creation timestamp |
| updated_at | TIMESTAMP | NULL | Update timestamp |

**Indexes**:
- Primary key on `id`
- Unique index on `token`
- Index on `user_id` (for finding user's tokens)
- Index on `expires_at` (for cleanup queries)

---

## API Endpoints

### 1. Login (Google/Apple)

**Updated Response**:

Both `/auth/google/login` and `/auth/apple/login` now return:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123def456...",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "name": "John Doe",
    "pictureUrl": "https://...",
    "oauthProvider": "google",
    "createdAt": "2025-11-15T10:00:00Z"
  },
  "expiresIn": 1800
}
```

### 2. Refresh Token

**Endpoint**: `POST /auth/refresh`

**Request**:
```json
{
  "refreshToken": "abc123def456..."
}
```

**Success Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_token_here...",
  "expiresIn": 1800
}
```

**Error Responses**:

- **400 Bad Request**: Invalid request format
- **401 Unauthorized**: Invalid, expired, or revoked refresh token
- **404 Not Found**: User not found
- **500 Internal Server Error**: Server error

**Example**:
```bash
curl -X POST http://localhost:8080/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "abc123def456..."
  }'
```

### 3. Logout

**Endpoint**: `POST /auth/logout`

**Headers**:
```
Authorization: Bearer <access-token>
```

**Success Response** (200 OK):
```json
{
  "message": "Logged out successfully"
}
```

**Description**: Revokes all refresh tokens for the authenticated user (logout from all devices).

**Example**:
```bash
curl -X POST http://localhost:8080/auth/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Frontend Integration

### Token Storage

**React Native Example** (using AsyncStorage):

```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';

// Store tokens after login
async function storeTokens(accessToken, refreshToken) {
  await AsyncStorage.multiSet([
    ['@access_token', accessToken],
    ['@refresh_token', refreshToken],
  ]);
}

// Retrieve tokens
async function getTokens() {
  const values = await AsyncStorage.multiGet(['@access_token', '@refresh_token']);
  return {
    accessToken: values[0][1],
    refreshToken: values[1][1],
  };
}

// Clear tokens on logout
async function clearTokens() {
  await AsyncStorage.multiRemove(['@access_token', '@refresh_token']);
}
```

### Automatic Token Refresh

**React Native with Axios Interceptor**:

```javascript
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

const api = axios.create({
  baseURL: 'http://localhost:8080',
});

// Request interceptor - add access token
api.interceptors.request.use(
  async (config) => {
    const accessToken = await AsyncStorage.getItem('@access_token');
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = await AsyncStorage.getItem('@refresh_token');
        
        // Refresh the token
        const { data } = await axios.post('http://localhost:8080/auth/refresh', {
          refreshToken,
        });

        // Store new tokens
        await AsyncStorage.multiSet([
          ['@access_token', data.accessToken],
          ['@refresh_token', data.refreshToken],
        ]);

        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed - redirect to login
        await AsyncStorage.multiRemove(['@access_token', '@refresh_token']);
        // Navigate to login screen
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

### Login Flow

```javascript
import api from './api';

async function signInWithGoogle() {
  const userInfo = await GoogleSignin.signIn();
  
  const { data } = await axios.post('http://localhost:8080/auth/google/login', {
    idToken: userInfo.idToken,
  });

  // Store both tokens
  await AsyncStorage.multiSet([
    ['@access_token', data.accessToken],
    ['@refresh_token', data.refreshToken],
    ['@user', JSON.stringify(data.user)],
  ]);

  return data.user;
}
```

### Logout Flow

```javascript
async function logout() {
  try {
    // Call logout endpoint to revoke refresh tokens
    await api.post('/auth/logout');
  } catch (error) {
    console.error('Logout error:', error);
  } finally {
    // Clear local tokens
    await AsyncStorage.multiRemove(['@access_token', '@refresh_token', '@user']);
    // Navigate to login screen
  }
}
```

---

## Security Features

### 1. Token Rotation

Every refresh token use generates a new refresh token and revokes the old one. This prevents:
- Replay attacks
- Token theft exploitation
- Long-term token compromise

### 2. Token Revocation

Tokens can be revoked:
- Individually (on refresh)
- All at once (logout from all devices)
- Automatically (on suspicious activity)

### 3. Expiration

- **Access Token**: 30 minutes (short-lived)
- **Refresh Token**: 90 days (long-lived)

### 4. Device Tracking

Refresh tokens store:
- Device name (from User-Agent header)
- IP address (from X-Forwarded-For or X-Real-IP)

This enables:
- Security monitoring
- Device management UI
- Suspicious activity detection

### 5. Database Cleanup

Expired tokens should be periodically cleaned:

```swift
// Run this as a scheduled task
try await refreshTokenService.deleteExpiredTokens(on: db)
```

---

## Configuration

### Access Token Expiration

Change in `App+build.swift`:

```swift
let jwtService = try JWTService(
    secret: jwtSecret,
    tokenExpirationTime: 30 * 60 // 30 minutes
)
```

### Refresh Token Expiration

Change in `App+build.swift`:

```swift
let refreshTokenService = RefreshTokenService(
    refreshTokenExpirationTime: 90 * 24 * 60 * 60 // 90 days
)
```

---

## Best Practices

### 1. Store Tokens Securely

**Mobile Apps**:
- iOS: Use Keychain
- Android: Use EncryptedSharedPreferences

**Web Apps**:
- Use httpOnly cookies for refresh tokens
- Store access tokens in memory (not localStorage)

### 2. Implement Token Rotation

✅ Already implemented - each refresh generates new tokens

### 3. Set Appropriate Expiration Times

- Access Token: 15-60 minutes
- Refresh Token: 30-90 days

### 4. Monitor for Suspicious Activity

Track:
- Multiple refresh token uses from different IPs
- Rapid token refresh attempts
- Token reuse after revocation

### 5. Implement Cleanup

Schedule periodic cleanup of expired tokens:

```swift
// Example: Run daily cleanup task
Task {
    while true {
        try await Task.sleep(for: .seconds(86400)) // 24 hours
        try await refreshTokenService.deleteExpiredTokens(on: db)
    }
}
```

---

## Testing

### Test Login and Receive Tokens

```bash
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "google-token"}' | jq
```

Expected:
```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "user": {...},
  "expiresIn": 1800
}
```

### Test Token Refresh

```bash
curl -X POST http://localhost:8080/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "your-refresh-token"}' | jq
```

### Test Logout

```bash
curl -X POST http://localhost:8080/auth/logout \
  -H "Authorization: Bearer your-access-token"
```

### Verify Token Revocation

After logout, trying to refresh should fail:

```bash
curl -X POST http://localhost:8080/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "revoked-token"}'
```

Expected: 401 Unauthorized

---

## Database Queries

### View All Refresh Tokens for a User

```sql
SELECT * FROM refresh_tokens 
WHERE user_id = 'user-uuid' 
  AND is_revoked = false 
  AND expires_at > NOW()
ORDER BY created_at DESC;
```

### Revoke All Tokens for a User

```sql
UPDATE refresh_tokens 
SET is_revoked = true 
WHERE user_id = 'user-uuid';
```

### Clean Up Expired Tokens

```sql
DELETE FROM refresh_tokens 
WHERE expires_at < NOW();
```

### Count Active Sessions

```sql
SELECT user_id, COUNT(*) as active_sessions
FROM refresh_tokens
WHERE is_revoked = false 
  AND expires_at > NOW()
GROUP BY user_id;
```

---

## Migration

The refresh token migration runs automatically on app startup.

**Manual Migration**:

```bash
# Already applied automatically when you run the app
swift run App
```

**Rollback** (if needed):

```sql
DROP TABLE refresh_tokens;
DELETE FROM _fluent_migrations WHERE name = 'CreateRefreshToken';
```

---

## Troubleshooting

### Issue: "Invalid refresh token"

**Causes**:
- Token has expired
- Token was revoked
- Token doesn't exist in database

**Solution**: User must log in again

### Issue: Multiple refresh tokens per user

**Cause**: Expected behavior - one per device/session

**Solution**: This is normal. Each login creates a new refresh token.

### Issue: Database growing large

**Cause**: Expired tokens not being cleaned up

**Solution**: Implement scheduled cleanup task

---

## Summary

✅ **Implemented**:
- Refresh token model and migration
- Token generation and verification service
- Login endpoints return both tokens
- `/auth/refresh` endpoint for token refresh
- `/auth/logout` endpoint for token revocation
- Automatic token rotation
- Device and IP tracking

✅ **Security Features**:
- Short-lived access tokens (30 min)
- Long-lived refresh tokens (90 days)
- Token rotation on refresh
- Token revocation support
- Database-backed token storage

✅ **Frontend Ready**:
- Clear API contracts
- Example integration code
- Automatic refresh interceptor pattern

---

**Next Steps**:
1. Integrate frontend with new token endpoints
2. Implement secure token storage
3. Set up token refresh interceptors
4. Test the complete flow
5. Configure appropriate expiration times for production
