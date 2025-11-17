# OTP Authentication Guide

## Overview

The RealinApi now supports **passwordless email authentication** using One-Time Passwords (OTP). Users can receive a 6-digit code via email and use it to authenticate without needing OAuth providers like Google or Apple.

## Features

- **6-Digit OTP Codes**: Secure, randomly generated codes
- **Time-Limited**: OTPs expire after 10 minutes
- **Rate Limiting**: Maximum 5 OTP requests per email per hour
- **Attempt Tracking**: Maximum 5 verification attempts per OTP
- **Auto-Cleanup**: Expired OTPs are automatically removed
- **User Auto-Registration**: New users are created automatically on successful OTP verification
- **Dual-Token System**: Returns both access token (30 min) and refresh token (90 days)

## API Endpoints

### 1. Send OTP

Request an OTP code to be sent to an email address.

**Endpoint**: `POST /auth/otp/send`

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

**Success Response** (200 OK):
```json
{
  "message": "OTP sent successfully to user@example.com",
  "expiresIn": 600
}
```

**Error Responses**:
- `400 Bad Request`: Invalid email format or missing email
- `429 Too Many Requests`: Rate limit exceeded (5 requests/hour)
- `500 Internal Server Error`: Failed to send email

**Rate Limiting**:
- Maximum 5 OTP requests per email address per hour
- Counter resets after 1 hour from first request

---

### 2. Verify OTP

Verify the OTP code and authenticate the user.

**Endpoint**: `POST /auth/otp/verify`

**Request Body**:
```json
{
  "email": "user@example.com",
  "code": "123456",
  "name": "John Doe"  // Optional: User's name (only used for new users)
}
```

**Success Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "user": {
    "id": "uuid-here",
    "email": "user@example.com",
    "name": "John Doe",
    "oauthProvider": "email",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "expiresIn": 1800
}
```

**Error Responses**:
- `400 Bad Request`: Invalid request format or missing required fields
- `401 Unauthorized`: Invalid OTP code, expired OTP, or too many attempts
- `500 Internal Server Error`: Server error

**Behavior**:
- If the user exists: Updates user info (if name provided) and returns tokens
- If the user is new: Creates a new user account and returns tokens
- Each OTP can be attempted maximum 5 times
- OTP is invalidated after successful verification

---

## Authentication Flow

```
┌─────────┐                   ┌─────────┐                   ┌──────────┐
│ Client  │                   │  API    │                   │ Database │
└────┬────┘                   └────┬────┘                   └────┬─────┘
     │                             │                             │
     │ POST /auth/otp/send         │                             │
     │ { email: "user@example.com" }│                            │
     ├────────────────────────────>│                             │
     │                             │                             │
     │                             │ Check rate limit            │
     │                             ├────────────────────────────>│
     │                             │                             │
     │                             │ Generate 6-digit code       │
     │                             │                             │
     │                             │ Save OTP to database        │
     │                             ├────────────────────────────>│
     │                             │                             │
     │                             │ Send email with code        │
     │                             │                             │
     │   200 OK                    │                             │
     │   { message: "OTP sent..." }│                             │
     │<────────────────────────────┤                             │
     │                             │                             │
     │ POST /auth/otp/verify       │                             │
     │ { email, code: "123456" }   │                             │
     ├────────────────────────────>│                             │
     │                             │                             │
     │                             │ Verify OTP code             │
     │                             ├────────────────────────────>│
     │                             │                             │
     │                             │ Find or create user         │
     │                             ├────────────────────────────>│
     │                             │                             │
     │                             │ Generate access token       │
     │                             │ Generate refresh token      │
     │                             ├────────────────────────────>│
     │                             │                             │
     │   200 OK                    │                             │
     │   { accessToken, refreshToken, user }                    │
     │<────────────────────────────┤                             │
     │                             │                             │
```

## Frontend Integration Examples

### React/TypeScript Example

```typescript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:8080';

interface OTPSendResponse {
  message: string;
  expiresIn: number;
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: {
    id: string;
    email: string;
    name?: string;
    oauthProvider?: string;
  };
  expiresIn: number;
}

// Step 1: Request OTP
async function sendOTP(email: string): Promise<OTPSendResponse> {
  const response = await axios.post(`${API_BASE_URL}/auth/otp/send`, {
    email
  });
  return response.data;
}

// Step 2: Verify OTP and login
async function verifyOTP(
  email: string, 
  code: string, 
  name?: string
): Promise<LoginResponse> {
  const response = await axios.post(`${API_BASE_URL}/auth/otp/verify`, {
    email,
    code,
    name
  });
  
  // Store tokens
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  
  return response.data;
}

// Complete login flow
async function loginWithOTP(email: string, code: string, name?: string) {
  try {
    // First, request OTP
    await sendOTP(email);
    console.log('OTP sent to', email);
    
    // Then verify (in real app, this would be separate after user enters code)
    const result = await verifyOTP(email, code, name);
    console.log('Logged in as', result.user.email);
    
    return result;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error('Login failed:', error.response?.data);
    }
    throw error;
  }
}
```

### Swift/iOS Example

```swift
import Foundation

struct OTPSendRequest: Codable {
    let email: String
}

struct OTPSendResponse: Codable {
    let message: String
    let expiresIn: Int
}

struct OTPVerifyRequest: Codable {
    let email: String
    let code: String
    let name: String?
}

struct LoginResponse: Codable {
    let accessToken: String
    let refreshToken: String
    let user: User
    let expiresIn: Int
}

struct User: Codable {
    let id: String
    let email: String
    let name: String?
    let oauthProvider: String?
}

class OTPAuthService {
    let baseURL = "http://localhost:8080"
    
    // Step 1: Request OTP
    func sendOTP(email: String) async throws -> OTPSendResponse {
        guard let url = URL(string: "\(baseURL)/auth/otp/send") else {
            throw URLError(.badURL)
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = OTPSendRequest(email: email)
        request.httpBody = try JSONEncoder().encode(body)
        
        let (data, _) = try await URLSession.shared.data(for: request)
        return try JSONDecoder().decode(OTPSendResponse.self, from: data)
    }
    
    // Step 2: Verify OTP
    func verifyOTP(email: String, code: String, name: String? = nil) async throws -> LoginResponse {
        guard let url = URL(string: "\(baseURL)/auth/otp/verify") else {
            throw URLError(.badURL)
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = OTPVerifyRequest(email: email, code: code, name: name)
        request.httpBody = try JSONEncoder().encode(body)
        
        let (data, _) = try await URLSession.shared.data(for: request)
        let response = try JSONDecoder().decode(LoginResponse.self, from: data)
        
        // Store tokens
        UserDefaults.standard.set(response.accessToken, forKey: "accessToken")
        UserDefaults.standard.set(response.refreshToken, forKey: "refreshToken")
        
        return response
    }
}

// Usage
Task {
    let service = OTPAuthService()
    
    do {
        // Step 1: Send OTP
        let sendResult = try await service.sendOTP(email: "user@example.com")
        print(sendResult.message)
        
        // Step 2: Verify OTP (after user enters code)
        let loginResult = try await service.verifyOTP(
            email: "user@example.com",
            code: "123456",
            name: "John Doe"
        )
        print("Logged in as \(loginResult.user.email)")
    } catch {
        print("Error: \(error)")
    }
}
```

## Security Considerations

### OTP Generation
- Uses secure random number generation
- 6-digit numeric codes (000000-999999)
- Each code is unique per email/timestamp

### Rate Limiting
- **5 requests per hour per email**: Prevents brute force attacks
- Tracks requests by email address
- Rate limit counter expires after 1 hour

### Attempt Tracking
- **Maximum 5 verification attempts** per OTP
- After 5 failed attempts, OTP is invalidated
- New OTP must be requested

### Expiration
- OTPs expire after **10 minutes**
- Expired OTPs cannot be verified
- Automatic cleanup removes old OTPs

### Email Validation
- Validates email format before sending OTP
- Prevents invalid email submissions

### IP Address Logging
- Stores IP address with each OTP request
- Helps track suspicious activity
- Supports X-Forwarded-For and X-Real-IP headers for proxy setups

## Database Schema

### OTPs Table

```sql
CREATE TABLE otps (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    code VARCHAR(6) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT false,
    attempts INTEGER NOT NULL DEFAULT 0,
    ip_address VARCHAR(45),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_otps_email ON otps(email);
CREATE INDEX idx_otps_expires_at ON otps(expires_at);
```

**Columns**:
- `id`: Unique identifier (UUID)
- `email`: User's email address
- `code`: 6-digit OTP code
- `expires_at`: Expiration timestamp (10 minutes from creation)
- `is_used`: Whether OTP has been successfully verified
- `attempts`: Number of verification attempts (max 5)
- `ip_address`: IP address of request (optional, for security tracking)
- `created_at`: Creation timestamp
- `updated_at`: Last update timestamp

**Indexes**:
- `email`: Fast lookup by email for rate limiting
- `expires_at`: Efficient cleanup of expired OTPs

## Email Service Configuration

### Development Mode (Console)

By default, the API uses `ConsoleEmailService` which prints OTPs to the console:

```swift
let emailService = ConsoleEmailService()
```

**Console Output**:
```
================================================================================
📧 EMAIL SERVICE (Console)
================================================================================
To: user@example.com
Subject: Your Login Code

Your verification code is: 123456

This code will expire in 10 minutes.
================================================================================
```

### Production Mode

For production, implement a real email service using SMTP or a service like SendGrid:

#### SendGrid Example

```swift
import AsyncHTTPClient

class SendGridEmailService: EmailServiceProtocol {
    let apiKey: String
    let httpClient: HTTPClient
    let fromEmail: String
    
    init(apiKey: String, httpClient: HTTPClient, fromEmail: String) {
        self.apiKey = apiKey
        self.httpClient = httpClient
        self.fromEmail = fromEmail
    }
    
    func sendOTP(to email: String, code: String) async throws {
        let url = "https://api.sendgrid.com/v3/mail/send"
        
        let body: [String: Any] = [
            "personalizations": [[
                "to": [["email": email]]
            ]],
            "from": ["email": fromEmail],
            "subject": "Your Login Code",
            "content": [[
                "type": "text/plain",
                "value": """
                Your verification code is: \(code)
                
                This code will expire in 10 minutes.
                
                If you didn't request this code, please ignore this email.
                """
            ]]
        ]
        
        let jsonData = try JSONSerialization.data(withJSONObject: body)
        
        var request = HTTPClientRequest(url: url)
        request.method = .POST
        request.headers.add(name: "Authorization", value: "Bearer \(apiKey)")
        request.headers.add(name: "Content-Type", value: "application/json")
        request.body = .bytes(ByteBuffer(data: jsonData))
        
        _ = try await httpClient.execute(request, timeout: .seconds(30))
    }
}
```

**Usage in App+build.swift**:
```swift
// Get SendGrid API key from environment
let sendGridApiKey = environment.get("SENDGRID_API_KEY") ?? ""
let fromEmail = environment.get("FROM_EMAIL") ?? "noreply@example.com"

// Use SendGrid in production
let emailService: EmailServiceProtocol = sendGridApiKey.isEmpty 
    ? ConsoleEmailService() 
    : SendGridEmailService(
        apiKey: sendGridApiKey, 
        httpClient: httpClient,
        fromEmail: fromEmail
      )
```

## Testing

### Manual Testing with cURL

#### Send OTP
```bash
curl -X POST http://localhost:8080/auth/otp/send \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com"
  }'
```

#### Verify OTP
```bash
# Check console output for the OTP code, then:
curl -X POST http://localhost:8080/auth/otp/verify \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "code": "123456",
    "name": "Test User"
  }'
```

### Unit Testing

```swift
import XCTest
@testable import App

class OTPAuthTests: XCTestCase {
    var app: Application!
    
    override func setUp() async throws {
        app = try await buildApplication(TestArguments())
    }
    
    override func tearDown() async throws {
        try await app.shutdown()
    }
    
    func testSendOTP() async throws {
        // Test sending OTP
        try await app.test(.POST, "/auth/otp/send") { request in
            try request.content.encode([
                "email": "test@example.com"
            ])
        } afterResponse: { response in
            XCTAssertEqual(response.status, .ok)
            
            let otpResponse = try response.content.decode(SendOTPResponse.self)
            XCTAssertTrue(otpResponse.message.contains("OTP sent successfully"))
            XCTAssertEqual(otpResponse.expiresIn, 600)
        }
    }
    
    func testVerifyOTP() async throws {
        // First send OTP
        try await app.test(.POST, "/auth/otp/send") { request in
            try request.content.encode(["email": "test@example.com"])
        }
        
        // Then verify (you'd need to mock the OTP service to know the code)
        try await app.test(.POST, "/auth/otp/verify") { request in
            try request.content.encode([
                "email": "test@example.com",
                "code": "123456",
                "name": "Test User"
            ])
        } afterResponse: { response in
            XCTAssertEqual(response.status, .ok)
            
            let loginResponse = try response.content.decode(LoginResponse.self)
            XCTAssertFalse(loginResponse.accessToken.isEmpty)
            XCTAssertFalse(loginResponse.refreshToken.isEmpty)
            XCTAssertEqual(loginResponse.user.email, "test@example.com")
        }
    }
}
```

## Troubleshooting

### OTP Not Received
- **Check console output**: In development, OTPs are printed to console
- **Verify email format**: Must be valid email address
- **Check rate limiting**: Maximum 5 requests per hour

### Invalid OTP Error
- **Check expiration**: OTPs expire after 10 minutes
- **Verify code**: Must be exact 6-digit code
- **Check attempts**: Maximum 5 attempts per OTP

### Rate Limit Error
- **Wait 1 hour**: Rate limit resets after 1 hour
- **Check database**: Review `otps` table for request history

### Database Errors
- **Run migrations**: Ensure `CreateOTP` migration has run
- **Check connection**: Verify PostgreSQL connection
- **Review logs**: Check application logs for detailed errors

## Best Practices

1. **Use HTTPS in Production**: Always use HTTPS to protect tokens and OTP codes
2. **Implement Email Service**: Replace `ConsoleEmailService` with real email provider
3. **Set Strong JWT Secret**: Use a strong, random secret key for JWT signing
4. **Monitor Rate Limits**: Track OTP requests for suspicious activity
5. **Cleanup Old OTPs**: Run periodic cleanup to remove expired OTPs
6. **Log Security Events**: Log failed OTP attempts and rate limit violations
7. **Add CAPTCHA**: Consider adding CAPTCHA to OTP request endpoint
8. **Email Templates**: Use HTML email templates for better user experience
9. **Localization**: Support multiple languages for email content
10. **Retry Logic**: Implement exponential backoff for email sending failures

## Migration Guide

If you're upgrading from OAuth-only authentication, the OTP system is fully backward compatible:

1. **Existing Users**: Can continue using Google/Apple sign-in
2. **New Users**: Can choose between OAuth or OTP
3. **Database**: Existing users have `oauth_provider` as "google" or "apple"
4. **OTP Users**: Have `oauth_provider` as "email"

No changes are required to existing authentication code. All three methods (Google, Apple, OTP) return the same `LoginResponse` format.
