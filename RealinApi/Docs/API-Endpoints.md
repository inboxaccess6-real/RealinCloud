# API Endpoints Reference

## Base URL

```
http://localhost:8080
```

For production, replace with your domain (e.g., `https://api.realin.com`).

---

## Public Endpoints

These endpoints do not require authentication.

### Health Check

Check if the API is running.

**Endpoint**: `GET /health`

**Request**: None

**Response** (200 OK):
```json
{
  "status": "ok",
  "timestamp": "2025-11-15T10:30:00Z"
}
```

---

### Root Endpoint

Basic API information.

**Endpoint**: `GET /`

**Request**: None

**Response** (200 OK):
```
RealinApi - OAuth Authentication Ready!
```

---

## Authentication Endpoints

The API supports three authentication methods:
1. **Google OAuth** - Sign in with Google
2. **Apple Sign-In** - Sign in with Apple
3. **OTP Email** - Passwordless login with email verification code

All authentication methods return the same response format with access token, refresh token, and user information.

---

### Google OAuth Login

Authenticate a user using a Google ID token.

**Endpoint**: `POST /auth/google/login`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjU5N..."
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| idToken | string | Yes | Google ID token obtained from Google Sign-In |

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "email": "user@gmail.com",
    "name": "John Doe",
    "pictureUrl": "https://lh3.googleusercontent.com/a/...",
    "oauthProvider": "google",
    "createdAt": "2025-11-15T10:30:00Z"
  }
}
```

**Error Responses**:

- **400 Bad Request**:
  ```json
  {
    "error": "Missing request body"
  }
  ```
  or
  ```json
  {
    "error": "Invalid request format"
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Invalid Google token: ..."
  }
  ```

- **500 Internal Server Error**:
  ```json
  {
    "error": "Database not available"
  }
  ```

**Example**:
```bash
curl -X POST http://localhost:8080/auth/google/login \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjU5N..."
  }'
```

---

### Apple OAuth Login

Authenticate a user using an Apple ID token.

**Endpoint**: `POST /auth/apple/login`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "idToken": "eyJraWQiOiJlWGF1bm1MIiwiYWxnIjoiUlMyNTYifQ...",
  "name": "John Doe"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| idToken | string | Yes | Apple ID token from Sign in with Apple |
| name | string | No | User's full name (provided only on first sign-in) |

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "a1b2c3d4-e5f6-4789-a012-b3c4d5e6f7a8",
    "email": "user@privaterelay.appleid.com",
    "name": "John Doe",
    "pictureUrl": null,
    "oauthProvider": "apple",
    "createdAt": "2025-11-15T10:30:00Z"
  }
}
```

**Error Responses**:

- **400 Bad Request**:
  ```json
  {
    "error": "Missing request body"
  }
  ```
  or
  ```json
  {
    "error": "Invalid request format"
  }
  ```
  or
  ```json
  {
    "error": "Email not provided by Apple"
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Invalid Apple token: ..."
  }
  ```

- **500 Internal Server Error**:
  ```json
  {
    "error": "Database not available"
  }
  ```

**Example**:
```bash
curl -X POST http://localhost:8080/auth/apple/login \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "eyJraWQiOiJlWGF1bm1MIiwiYWxnIjoiUlMyNTYifQ...",
    "name": "John Doe"
  }'
```

---

### OTP Email Login - Send OTP

Request an OTP code to be sent to an email address.

**Endpoint**: `POST /auth/otp/send`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| email | string | Yes | Valid email address to receive OTP |

**Success Response** (200 OK):
```json
{
  "message": "OTP sent successfully to user@example.com",
  "expiresIn": 600
}
```

**Error Responses**:

- **400 Bad Request**:
  ```json
  {
    "error": "Invalid email format"
  }
  ```

- **429 Too Many Requests**:
  ```json
  {
    "error": "Too many OTP requests. Please try again later"
  }
  ```

- **500 Internal Server Error**:
  ```json
  {
    "error": "Failed to send OTP email"
  }
  ```

**Example**:
```bash
curl -X POST http://localhost:8080/auth/otp/send \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com"
  }'
```

**Rate Limiting**: Maximum 5 OTP requests per email per hour.

---

### OTP Email Login - Verify OTP

Verify the OTP code and authenticate the user.

**Endpoint**: `POST /auth/otp/verify`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "user@example.com",
  "code": "123456",
  "name": "John Doe"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| email | string | Yes | Email address that received the OTP |
| code | string | Yes | 6-digit OTP code |
| name | string | No | User's name (only used for new users) |

**Success Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "user": {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "email": "user@example.com",
    "name": "John Doe",
    "pictureUrl": null,
    "oauthProvider": "email",
    "createdAt": "2025-11-15T10:30:00Z",
    "updatedAt": "2025-11-15T10:30:00Z"
  },
  "expiresIn": 1800
}
```

**Error Responses**:

- **400 Bad Request**:
  ```json
  {
    "error": "Invalid request format"
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Invalid OTP code"
  }
  ```
  or
  ```json
  {
    "error": "OTP has expired"
  }
  ```
  or
  ```json
  {
    "error": "Maximum verification attempts exceeded"
  }
  ```

- **500 Internal Server Error**:
  ```json
  {
    "error": "Database not available"
  }
  ```

**Example**:
```bash
curl -X POST http://localhost:8080/auth/otp/verify \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "code": "123456",
    "name": "John Doe"
  }'
```

**Notes**:
- OTPs expire after 10 minutes
- Maximum 5 verification attempts per OTP
- If user exists, updates their information (if name provided)
- If user is new, creates a new account automatically

---

## Protected Endpoints

These endpoints require a valid JWT token in the `Authorization` header.

### Get Current User

Retrieve the authenticated user's profile information.

**Endpoint**: `GET /auth/me`

**Headers**:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Request**: None

**Success Response** (200 OK):
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "email": "user@gmail.com",
  "name": "John Doe",
  "pictureUrl": "https://lh3.googleusercontent.com/a/...",
  "oauthProvider": "google",
  "createdAt": "2025-11-15T10:30:00Z"
}
```

**Error Responses**:

- **401 Unauthorized**:
  ```json
  {
    "error": "Missing authorization header"
  }
  ```
  or
  ```json
  {
    "error": "Invalid or expired token"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "User not found"
  }
  ```

- **500 Internal Server Error**:
  ```json
  {
    "error": "Database not available"
  }
  ```

**Example**:
```bash
curl http://localhost:8080/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Data Models

### User Object

```json
{
  "id": "uuid",
  "email": "string",
  "name": "string | null",
  "pictureUrl": "string | null",
  "oauthProvider": "google | apple | email",
  "createdAt": "ISO 8601 datetime string",
  "updatedAt": "ISO 8601 datetime string"
}
```

| Field | Type | Description |
|-------|------|-------------|
| id | UUID | Unique user identifier |
| email | string | User's email address |
| name | string? | User's full name (optional) |
| pictureUrl | string? | URL to user's profile picture (optional) |
| oauthProvider | string | Authentication provider ("google", "apple", or "email") |
| createdAt | string | ISO 8601 timestamp of account creation |
| updatedAt | string | ISO 8601 timestamp of last update |

### Login Response

```json
{
  "token": "string",
  "user": "User Object"
}
```

| Field | Type | Description |
|-------|------|-------------|
| token | string | JWT token for authentication |
| user | User | User object with profile information |

### Error Response

```json
{
  "error": "string"
}
```

| Field | Type | Description |
|-------|------|-------------|
| error | string | Human-readable error message |

---

## HTTP Status Codes

| Code | Description | When It Occurs |
|------|-------------|----------------|
| 200 | OK | Request succeeded |
| 400 | Bad Request | Invalid request format or missing required fields |
| 401 | Unauthorized | Invalid, missing, or expired authentication token |
| 404 | Not Found | Requested resource doesn't exist |
| 500 | Internal Server Error | Server-side error (database, network, etc.) |

---

## Authentication Flow

### Step 1: Obtain OAuth Token from Provider

**Google**: Use Google Sign-In SDK in your frontend
**Apple**: Use Sign in with Apple SDK in your frontend

### Step 2: Send Token to Backend

Send the ID token to `/auth/google/login` or `/auth/apple/login`

### Step 3: Receive JWT Token

Backend returns a JWT token in the response

### Step 4: Store JWT Token

Store the token securely (e.g., secure storage, keychain)

### Step 5: Include Token in Requests

Add `Authorization: Bearer <token>` header to all protected endpoints

### Step 6: Handle Token Expiration

Tokens expire after 30 days (configurable). When you receive a 401 error, prompt the user to sign in again.

---

## Rate Limiting

**Note**: Rate limiting is not currently implemented but should be added for production.

Recommended limits:
- Login endpoints: 5 requests per minute per IP
- Protected endpoints: 100 requests per minute per user

---

## CORS

If you're calling the API from a web frontend, ensure CORS is properly configured:

```swift
// Add to App+build.swift
router.addMiddleware {
    CORSMiddleware(
        allowedOrigins: ["https://your-frontend.com"],
        allowedMethods: [.GET, .POST],
        allowedHeaders: [.authorization, .contentType]
    )
}
```

---

## Versioning

Current API Version: **v1** (implicit)

Future versions should use URL prefixing: `/v2/auth/google/login`

---

## Testing with Postman

### Import Collection

Create a new Postman collection with the following requests:

1. **Health Check**: `GET http://localhost:8080/health`
2. **Google Login**: `POST http://localhost:8080/auth/google/login`
3. **Apple Login**: `POST http://localhost:8080/auth/apple/login`
4. **Get Me**: `GET http://localhost:8080/auth/me`

### Environment Variables

Create a Postman environment with:
- `base_url`: `http://localhost:8080`
- `jwt_token`: (set this after login)

### Auto-save Token

Add to login requests' Test script:
```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.environment.set("jwt_token", jsonData.token);
}
```

Use `{{jwt_token}}` in Authorization header for protected routes.

---

## Best Practices

### 1. Always Use HTTPS in Production

Never send tokens over unencrypted HTTP connections.

### 2. Token Storage

**Mobile Apps**: Use secure storage (iOS Keychain, Android Keystore)
**Web Apps**: Use httpOnly cookies or secure storage libraries

### 3. Token Refresh

Implement token refresh mechanism before tokens expire to avoid forcing users to re-authenticate.

### 4. Error Handling

Always handle error responses gracefully and provide user-friendly messages.

### 5. Retry Logic

Implement exponential backoff for network errors:
```javascript
async function loginWithRetry(idToken, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await login(idToken);
    } catch (error) {
      if (i === maxRetries - 1) throw error;
      await sleep(Math.pow(2, i) * 1000); // Exponential backoff
    }
  }
}
```

---

## Support

For issues or questions about the API, contact your development team.
