# Apple Sign-In Authentication

## Overview

The Apple authentication service validates Apple ID tokens using Apple's public keys (JWKS - JSON Web Key Set).

## How It Works

1. **Client sends Apple ID token** from Sign in with Apple
2. **Read JWT header** to extract the `kid` (key ID)
3. **Fetch Apple's public keys** from `https://appleid.apple.com/auth/keys`
4. **Cache keys** for 24 hours to reduce API calls
5. **Find matching key** by comparing `kid` values
6. **Validate token signature** using RSA public key
7. **Validate claims**:
   - Issuer: `https://appleid.apple.com`
   - Audience: Your app's bundle ID/client ID
   - Expiration time
   - Required claims: `sub`, `email`

## Configuration

Add your Apple Client ID (bundle ID) to `appsettings.json`:

```json
{
  "OAuth": {
    "Apple": {
      "ClientId": "com.yourcompany.yourapp"
    }
  }
}
```

For production, use User Secrets:

```bash
dotnet user-secrets set "Apple:ClientId" "com.yourcompany.yourapp"
```

## Security Features

✅ **Signature Validation**: Verifies token was issued by Apple  
✅ **Key Caching**: 24-hour cache reduces external API calls  
✅ **Issuer Validation**: Ensures token is from `appleid.apple.com`  
✅ **Audience Validation**: Confirms token is for your app  
✅ **Expiration Check**: Rejects expired tokens  
✅ **Clock Skew**: 5-minute tolerance for time differences  

## Dependencies

- `Microsoft.IdentityModel.Tokens` - JWT validation
- `IHttpClientFactory` - Fetch Apple's public keys
- `IMemoryCache` - Cache public keys

## Usage

The service is already registered in DI and used by `AuthService`:

```csharp
var payload = await _appleAuthService.ValidateAppleTokenAsync(idToken);
if (payload == null)
{
    return Results.Unauthorized();
}

// Use payload.Sub, payload.Email, payload.Name
```

## Apple Public Keys Response

Example JWKS from Apple:

```json
{
  "keys": [
    {
      "kty": "RSA",
      "kid": "W6WcOKB",
      "use": "sig",
      "alg": "RS256",
      "n": "...", // Modulus (base64url)
      "e": "AQAB" // Exponent (base64url)
    }
  ]
}
```

## Error Handling

- **No matching key**: Token's `kid` not in Apple's key set
- **Invalid signature**: Token tampered or not from Apple
- **Expired token**: User needs to sign in again
- **Missing claims**: Required `sub` or `email` not present
- **Network errors**: Failed to fetch Apple's keys (cached keys used if available)

## Testing

To test locally, you need:
1. Real Apple ID token from iOS/macOS app
2. Configured bundle ID in Apple Developer Console
3. Bundle ID matching your `Apple:ClientId` configuration

**Note**: Apple tokens cannot be easily mocked due to signature validation.
