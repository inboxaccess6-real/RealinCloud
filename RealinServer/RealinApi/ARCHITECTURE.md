# RealinApi Architecture Overview

## Authentication Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENT (Mobile App)                          │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP Request
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         ENDPOINTS LAYER                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  /api/auth/google    - Google OAuth Login                   │   │
│  │  /api/auth/apple     - Apple OAuth Login                    │   │
│  │  /api/auth/otp/request  - Request OTP via SMS/Email         │   │
│  │  /api/auth/otp/verify   - Verify OTP and Login              │   │
│  │  /api/auth/refresh      - Refresh Access Token              │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              │ Calls
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         SERVICE LAYER                                │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              AuthService (Business Logic)                   │   │
│  │  • Validates OAuth tokens                                   │   │
│  │  • Manages OTP generation/verification                      │   │
│  │  • Creates/updates users                                    │   │
│  │  • Generates JWT tokens                                     │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                              │
                ┌─────────────┼─────────────┐
                │             │             │
                ▼             ▼             ▼
┌──────────────────┐ ┌──────────────┐ ┌──────────────────┐
│ GoogleAuthService│ │  OtpService  │ │   JwtService     │
│  Validates Google│ │  Generates & │ │  Creates Access  │
│  ID tokens       │ │  Validates   │ │  & Refresh Tokens│
└──────────────────┘ │  OTP codes   │ └──────────────────┘
                     │  Rate Limits │
┌──────────────────┐ └──────────────┘ ┌──────────────────┐
│ AppleAuthService │                  │  SmsService      │
│  Validates Apple │                  │  EmailService    │
│  ID tokens       │                  │  (Send OTP)      │
└──────────────────┘                  └──────────────────┘
                              │
                              │ Persists Data
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         DATA LAYER                                   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    AppDbContext (EF Core)                   │   │
│  │  ┌───────────┐  ┌──────────────┐  ┌────────────────┐      │   │
│  │  │   Users   │  │RefreshTokens │  │  OtpSessions   │      │   │
│  │  │           │  │              │  │                │      │   │
│  │  │ • id      │  │ • id         │  │ • id           │      │   │
│  │  │ • email   │  │ • token      │  │ • email/phone  │      │   │
│  │  │ • provider│  │ • user_id    │  │ • otp_code     │      │   │
│  │  │ • oauth_id│  │ • expires_at │  │ • expires_at   │      │   │
│  │  └───────────┘  └──────────────┘  │ • is_verified  │      │   │
│  │                                    │ • attempt_count│      │   │
│  │                                    └────────────────┘      │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                      PostgreSQL Database
```

## Google/Apple Login Flow

```
1. Client App
   └─> Gets ID token from Google/Apple SDK
       └─> POST /api/auth/google or /api/auth/apple
           └─> AuthService validates token with provider
               └─> If valid:
                   ├─> Check if user exists (by oauth_provider_id)
                   │   ├─> Yes: Update user info
                   │   └─> No: Create new user
                   └─> Generate JWT access & refresh tokens
                       └─> Return tokens + user info to client
```

## OTP Login Flow

```
1. Client App
   └─> POST /api/auth/otp/request
       └─> OtpService
           ├─> Check rate limit (5 attempts per 60 min)
           │   └─> If exceeded: Return 429 error
           ├─> Generate 6-digit OTP code
           ├─> Save to OtpSessions table
           └─> Send via SMS/Email service
               └─> Return "OTP sent" message

2. User receives OTP code via SMS/Email

3. Client App
   └─> POST /api/auth/otp/verify
       └─> OtpService
           ├─> Validate OTP code
           │   ├─> Check expiration (10 minutes)
           │   ├─> Check attempt count (max 3)
           │   └─> Verify code matches
           ├─> Check if user exists by email/phone
           │   ├─> Yes: Load existing user
           │   └─> No: Create new OTP user
           └─> Generate JWT access & refresh tokens
               └─> Return tokens + user info to client
```

## Token Refresh Flow

```
Client App
└─> POST /api/auth/refresh
    └─> AuthService
        ├─> Validate refresh token
        │   ├─> Check token exists
        │   ├─> Check not revoked
        │   └─> Check not expired
        ├─> Revoke old refresh token
        ├─> Generate new access & refresh tokens
        └─> Return new tokens to client
```

## Key Features

### Rate Limiting (OTP)
- **Window**: 60 minutes (configurable)
- **Max Attempts**: 5 (configurable)
- **Scope**: Per email/phone number
- **Implementation**: Query OtpSessions created within time window

### User Provider Constraint
- Each user has ONE provider: Google, Apple, or OTP
- Unique index on (provider, oauth_provider_id)
- Email/phone uniqueness enforced

### Token Expiration
- **Access Token**: 60 minutes (configurable)
- **Refresh Token**: 30 days
- **OTP Code**: 10 minutes (configurable)

### Security
- JWT signed with HS256 algorithm
- Refresh tokens are base64-encoded random bytes
- OTP codes are cryptographically random
- All timestamps in UTC

## Endpoint Groups

All authentication endpoints are grouped under `/api/auth`:

```
/api/auth/
├── google          POST - Google OAuth login
├── apple           POST - Apple OAuth login
├── otp/
│   ├── request     POST - Request OTP
│   └── verify      POST - Verify OTP
└── refresh         POST - Refresh access token
```

## Database Indexes

**Users:**
- Primary: id (UUID)
- Unique: email
- Unique: phone_number
- Unique: (provider, oauth_provider_id)

**RefreshTokens:**
- Primary: id (UUID)
- Unique: token
- Index: user_id (FK)

**OtpSessions:**
- Primary: id (UUID)
- Index: email
- Index: phone_number
- Index: created_at (for rate limiting queries)
