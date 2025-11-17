# Mobile OTP Authentication - Migration Summary

## Overview

Successfully migrated from **email-based OTP authentication** to **mobile number (phone) based OTP authentication**. This change aligns with the three login options presented to users:

1. **Google Sign-In** (OAuth)
2. **Apple Sign-In** (OAuth)
3. **Mobile Number + OTP** (Passwordless)

## Key Changes

### Authentication Methods
Users can now authenticate using **any ONE** of these methods:
- Google OAuth (provides email)
- Apple Sign-In (provides email)
- Phone Number + OTP (provides phone number)

**Important**: Email is now optional and only provided by OAuth users. Phone-only users will not have an email address.

---

## Database Schema Changes

### User Table (`users`)

**Modified Fields**:
- `email` - Changed from **REQUIRED** to **OPTIONAL**
  - Reason: Phone-only users don't have email addresses
  - Unique constraint: Only enforced where email is not null and not empty
  
- `phone_number` - **NEW FIELD** (optional)
  - Type: VARCHAR(255)
  - Unique constraint: Enforced where phone_number is not null
  - Used for phone authentication

**Updated Schema**:
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY,
    oauth_provider VARCHAR(50),          -- 'google', 'apple', or 'phone'
    oauth_id VARCHAR(255),                -- OAuth user ID (null for phone auth)
    email VARCHAR(255),                   -- Optional (null for phone users)
    phone_number VARCHAR(255),            -- Optional (null for OAuth users)
    name VARCHAR(255),
    picture_url TEXT,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Unique indexes
CREATE UNIQUE INDEX idx_users_email 
  ON users(email) 
  WHERE email IS NOT NULL AND email != '';

CREATE UNIQUE INDEX idx_users_phone_number 
  ON users(phone_number) 
  WHERE phone_number IS NOT NULL;

CREATE UNIQUE INDEX idx_users_oauth 
  ON users(oauth_provider, oauth_id) 
  WHERE oauth_provider IS NOT NULL AND oauth_id IS NOT NULL;
```

### OTP Table (`otps`)

**Modified Fields**:
- `email` - **REMOVED**
- `phone_number` - **ADDED** (replaces email)
  - Type: VARCHAR(255)
  - Required field
  - Expected format: E.164 (e.g., +14155552671)

**Updated Schema**:
```sql
CREATE TABLE otps (
    id UUID PRIMARY KEY,
    phone_number VARCHAR(255) NOT NULL,  -- Changed from 'email'
    code VARCHAR(6) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT false,
    attempts INTEGER NOT NULL DEFAULT 0,
    ip_address VARCHAR(45),
    created_at TIMESTAMP NOT NULL
);

-- Updated indexes
CREATE INDEX idx_otps_phone_number ON otps(phone_number);
CREATE INDEX idx_otps_expires_at ON otps(expires_at);
```

---

## Code Changes

### Models

#### `User.swift`
```swift
// Updated OAuthProvider enum
enum OAuthProvider: String, Codable {
    case google
    case apple
    case phone    // Changed from 'email'
}

// Modified fields
@OptionalField(key: "email")           // Changed from @Field (required)
var email: String?

@OptionalField(key: "phone_number")    // NEW
var phoneNumber: String?

// New convenience initializer
convenience init(phoneNumber: String, name: String? = nil) {
    self.init(
        oauthProvider: .phone,
        oauthId: nil,
        email: nil,              // No email for phone users
        phoneNumber: phoneNumber,
        name: name
    )
}
```

#### `OTP.swift`
```swift
// Changed field
@Field(key: "phone_number")    // Was: @Field(key: "email")
var phoneNumber: String

// Updated initializer
init(
    id: UUID? = nil,
    phoneNumber: String,       // Was: email
    code: String,
    expiresAt: Date,
    ipAddress: String? = nil
)
```

### Services

#### `OTPService.swift`
**All methods updated** to use `phoneNumber` instead of `email`:
- `generateOTP(phoneNumber:ipAddress:on:)` - Generate OTP for phone
- `verifyOTP(phoneNumber:code:on:)` - Verify OTP for phone
- `checkRateLimit(phoneNumber:on:)` - Check rate limit per phone
- `invalidateExistingOTPs(phoneNumber:on:)` - Invalidate old OTPs

**Error Types**:
- `smsSendFailed` - Changed from `emailSendFailed`

#### `SMSService.swift` (renamed from `EmailService.swift`)
```swift
// New protocol
protocol SMSServiceProtocol: Sendable {
    func sendOTP(to phoneNumber: String, code: String) async throws
}

// Development implementations
actor ConsoleSMSService: SMSServiceProtocol {
    func sendOTP(to phoneNumber: String, code: String) async throws {
        print("📱 SMS to \(phoneNumber): Your code is \(code)")
    }
}

actor MockSMSService: SMSServiceProtocol {
    // For unit testing
}

// Production examples (commented out)
// - TwilioSMSService (using Twilio API)
// - AWSSNSSMSService (using AWS SNS)
```

### Controllers

#### `AuthController.swift`

**Updated DTOs**:
```swift
struct SendOTPRequest: Codable {
    let phoneNumber: String    // Was: email
}

struct VerifyOTPRequest: Codable {
    let phoneNumber: String    // Was: email
    let code: String
    let name: String?
}
```

**Updated Endpoints**:

1. **POST /auth/otp/send**
   ```swift
   // Request
   { "phoneNumber": "+14155552671" }
   
   // Response
   { "message": "OTP sent successfully to +14155552671", "expiresIn": 600 }
   ```

2. **POST /auth/otp/verify**
   ```swift
   // Request
   { 
     "phoneNumber": "+14155552671", 
     "code": "123456",
     "name": "John Doe"  // optional
   }
   
   // Response
   {
     "accessToken": "...",
     "refreshToken": "...",
     "user": {
       "id": "...",
       "email": null,              // null for phone users
       "phoneNumber": "+14155552671",
       "name": "John Doe",
       "oauthProvider": "phone",
       ...
     },
     "expiresIn": 1800
   }
   ```

**New Validation**:
```swift
// E.164 phone number format validation
private func isValidPhoneNumber(_ phoneNumber: String) -> Bool {
    let phoneRegex = "^\\+[1-9]\\d{9,14}$"
    return NSPredicate(format: "SELF MATCHES %@", phoneRegex).evaluate(with: phoneNumber)
}
```

**User Lookup Logic**:
```swift
// Find user by phone number
let existingUser = try await User.query(on: db)
    .filter(\.$phoneNumber == verifyRequest.phoneNumber)
    .first()

// Create new phone user
let user = User(
    phoneNumber: verifyRequest.phoneNumber,
    name: verifyRequest.name
)
```

### Configuration

#### `App+build.swift`
```swift
// Updated service initialization
let otpService = OTPService()           // For phone authentication
let smsService = ConsoleSMSService()    // For development

// Updated AuthController initialization
let authController = AuthController(
    googleOAuthService: googleOAuthService,
    appleOAuthService: appleOAuthService,
    jwtService: jwtService,
    refreshTokenService: refreshTokenService,
    otpService: otpService,
    smsService: smsService              // Changed from emailService
)
```

### Migrations

#### `CreateUser.swift`
- Made `email` optional (no `.required` constraint)
- Added `phone_number` field
- Added unique index on `phone_number` (where not null)
- Updated email unique index to exclude empty strings

#### `CreateOTP.swift`
- Changed `email` field to `phone_number`
- Updated index from `idx_otps_email` to `idx_otps_phone_number`

---

## Phone Number Format

### E.164 Standard
All phone numbers should follow the **E.164 international format**:

**Format**: `+[country code][subscriber number]`

**Examples**:
- USA: `+14155552671`
- UK: `+447700900123`
- India: `+919876543210`

**Validation**:
- Must start with `+`
- Followed by country code (1-3 digits)
- Total length: 10-15 digits (including country code)

**Frontend Guidance**:
Use libraries like `libphonenumber` to format and validate phone numbers before sending to API.

---

## User Authentication Flow

### Phone OTP Flow

1. **User enters phone number** → Frontend sends to `/auth/otp/send`
2. **Backend generates OTP** → Stores in database, sends via SMS
3. **User receives SMS** → Enters 6-digit code
4. **Frontend sends code** → `/auth/otp/verify`
5. **Backend verifies OTP** → Creates/finds user, generates tokens
6. **Returns tokens** → Access token (30 min) + Refresh token (90 days)

### Google/Apple OAuth Flow

1. **User signs in with Google/Apple** → Gets OAuth token
2. **Frontend sends OAuth token** → `/auth/google/login` or `/auth/apple/login`
3. **Backend verifies token** → Creates/finds user with email
4. **Returns tokens** → Access token + Refresh token

### User Data by Auth Method

| Auth Method | email | phoneNumber | oauthProvider | oauthId |
|-------------|-------|-------------|---------------|---------|
| Google | ✅ Required | ❌ null | 'google' | ✅ Google ID |
| Apple | ✅ Required | ❌ null | 'apple' | ✅ Apple ID |
| Phone OTP | ❌ null | ✅ Required | 'phone' | ❌ null |

---

## Migration Steps for Existing Deployments

### If You Have Existing Data

1. **Backup Database**
   ```bash
   pg_dump realin_db > backup_$(date +%Y%m%d).sql
   ```

2. **Add Phone Number Column**
   ```sql
   ALTER TABLE users ADD COLUMN phone_number VARCHAR(255);
   ```

3. **Make Email Optional**
   ```sql
   ALTER TABLE users ALTER COLUMN email DROP NOT NULL;
   ```

4. **Update OTP Table**
   ```sql
   ALTER TABLE otps RENAME COLUMN email TO phone_number;
   DROP INDEX idx_otps_email;
   CREATE INDEX idx_otps_phone_number ON otps(phone_number);
   ```

5. **Add Unique Indexes**
   ```sql
   CREATE UNIQUE INDEX idx_users_email 
     ON users(email) 
     WHERE email IS NOT NULL AND email != '';
   
   CREATE UNIQUE INDEX idx_users_phone_number 
     ON users(phone_number) 
     WHERE phone_number IS NOT NULL;
   ```

### If Starting Fresh

Simply run the application with updated migrations:
```bash
swift build
swift run
```

The migrations will create tables with the new schema automatically.

---

## Testing

### Development Testing

**Send OTP** (Console will print the code):
```bash
curl -X POST http://localhost:8080/auth/otp/send \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber": "+14155552671"}'
```

**Console Output**:
```
================================================================================
📱 SMS SERVICE (Console)
================================================================================
To: +14155552671
Message:

Your verification code is: 123456

This code will expire in 10 minutes.
================================================================================
```

**Verify OTP**:
```bash
curl -X POST http://localhost:8080/auth/otp/verify \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+14155552671",
    "code": "123456",
    "name": "John Doe"
  }'
```

**Response**:
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc-123...",
  "user": {
    "id": "...",
    "email": null,
    "phoneNumber": "+14155552671",
    "name": "John Doe",
    "pictureUrl": null,
    "oauthProvider": "phone",
    "createdAt": "2025-11-15T...",
    "updatedAt": "2025-11-15T..."
  },
  "expiresIn": 1800
}
```

---

## Production Deployment

### SMS Service Configuration

Replace `ConsoleSMSService` with a production SMS service:

#### Option 1: Twilio
```swift
// In App+build.swift
let smsService = TwilioSMSService(
    accountSid: environment.get("TWILIO_ACCOUNT_SID")!,
    authToken: environment.get("TWILIO_AUTH_TOKEN")!,
    fromPhoneNumber: environment.get("TWILIO_PHONE_NUMBER")!,
    httpClient: httpClient
)
```

**Environment Variables**:
```bash
export TWILIO_ACCOUNT_SID="AC..."
export TWILIO_AUTH_TOKEN="your_auth_token"
export TWILIO_PHONE_NUMBER="+14155552671"
```

#### Option 2: AWS SNS
```swift
import SotoSNS

let snsClient = SNS(...)
let smsService = AWSSNSSMSService(snsClient: snsClient)
```

---

## Security Considerations

1. **Phone Number Verification**: SMS OTPs verify ownership of phone number
2. **Rate Limiting**: Max 5 OTP requests per phone per hour
3. **Attempt Tracking**: Max 5 verification attempts per OTP
4. **Expiration**: OTPs expire after 10 minutes
5. **E.164 Format**: Standardized international phone format
6. **IP Logging**: Track request origin for security

---

## Frontend Integration Examples

### React/TypeScript
```typescript
// Send OTP
const sendOTP = async (phoneNumber: string) => {
  const response = await fetch('/auth/otp/send', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ phoneNumber })
  });
  return response.json();
};

// Verify OTP
const verifyOTP = async (phoneNumber: string, code: string, name?: string) => {
  const response = await fetch('/auth/otp/verify', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ phoneNumber, code, name })
  });
  const data = await response.json();
  
  // Store tokens
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  
  return data;
};
```

### Swift/iOS
```swift
// Phone number with country code picker
import PhoneNumberKit

let phoneNumberKit = PhoneNumberKit()
let phoneNumber = try phoneNumberKit.parse("+14155552671")
let e164 = phoneNumberKit.format(phoneNumber, toType: .e164)

// Send OTP
let request = SendOTPRequest(phoneNumber: e164)
let response = try await authService.sendOTP(request)

// Verify OTP
let verifyRequest = VerifyOTPRequest(
    phoneNumber: e164,
    code: "123456",
    name: "John Doe"
)
let loginResponse = try await authService.verifyOTP(verifyRequest)
```

---

## Breaking Changes

### API Changes
- ❌ `/auth/otp/send` - Now requires `phoneNumber` instead of `email`
- ❌ `/auth/otp/verify` - Now requires `phoneNumber` instead of `email`
- ✅ User response includes `phoneNumber` field
- ✅ `oauthProvider` can now be `"phone"` instead of `"email"`

### Database Schema
- ✅ `users.email` - Now nullable
- ✅ `users.phone_number` - New field
- ✅ `otps.phone_number` - Replaced `otps.email`

### Service Interfaces
- ❌ `EmailService` - Removed
- ✅ `SMSService` - New service

---

## Rollback Plan

If you need to rollback to email-based OTP:

1. Restore database from backup
2. Revert code changes (git revert)
3. Redeploy previous version

---

## Summary

✅ **Completed**:
- Migrated OTP from email to mobile numbers
- Updated all models, services, and controllers
- Modified database schema and migrations
- Added phone number validation (E.164 format)
- Created SMS service with console output for dev
- Updated all documentation

🎯 **Three Login Options**:
1. Google Sign-In (OAuth with email)
2. Apple Sign-In (OAuth with email)  
3. Mobile OTP (Phone verification)

📱 **Phone Format**: E.164 (e.g., +14155552671)

🚀 **Ready for Production**: Just configure SMS service (Twilio/AWS SNS)
