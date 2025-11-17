import AsyncHTTPClient
import Foundation
@preconcurrency import JWTKit
import NIOCore

/// Apple ID token claims structure
struct AppleIDTokenClaims: JWTPayload, @unchecked Sendable {
    let iss: IssuerClaim // Issuer (should be "https://appleid.apple.com")
    let sub: SubjectClaim // Apple user ID (unique, stable)
    let aud: AudienceClaim // Your app's client ID
    let exp: ExpirationClaim // Expiration time
    let iat: IssuedAtClaim // Issued at time
    let email: String? // User's email (optional)
    let emailVerified: String? // Email verification status
    
    enum CodingKeys: String, CodingKey {
        case iss
        case sub
        case aud
        case exp
        case iat
        case email
        case emailVerified = "email_verified"
    }
    
    func verify(using signer: JWTSigner) throws {
        // Verify expiration
        try exp.verifyNotExpired()
        
        // Verify issuer is Apple
        guard iss.value == "https://appleid.apple.com" else {
            throw JWTError.claimVerificationFailure(
                name: "iss",
                reason: "Token not issued by Apple"
            )
        }
    }
}

/// Apple's public key response structure
struct ApplePublicKeysResponse: Codable {
    let keys: [ApplePublicKey]
}

struct ApplePublicKey: Codable {
    let kty: String
    let kid: String
    let use: String
    let alg: String
    let n: String
    let e: String
}

/// Service for verifying Apple Sign In tokens
actor AppleOAuthService {
    private let httpClient: HTTPClient
    private let publicKeysEndpoint = "https://appleid.apple.com/auth/keys"
    private let appBundleId: String
    
    // Cache for Apple's public keys
    private var publicKeys: ApplePublicKeysResponse?
    private var lastKeyFetch: Date?
    private let keysCacheDuration: TimeInterval = 3600 // 1 hour
    
    init(httpClient: HTTPClient, appBundleId: String) {
        self.httpClient = httpClient
        self.appBundleId = appBundleId
    }
    
    /// Verify an Apple ID token and return user information
    /// - Parameter idToken: The Apple ID token from the frontend
    /// - Returns: AppleIDTokenClaims containing user details
    /// - Throws: Error if token is invalid or verification fails
    func verifyToken(_ idToken: String) async throws -> AppleIDTokenClaims {
        // Fetch Apple's public keys if needed
        let keys = try await fetchPublicKeys()
        
        // Parse the JWT header to get the key ID (kid)
        let components = idToken.split(separator: ".")
        guard components.count == 3 else {
            throw OAuthError.invalidToken(reason: "Invalid JWT format")
        }
        
        // Decode the header to get the kid
        guard let headerData = Data(base64URLEncoded: String(components[0])) else {
            throw OAuthError.invalidToken(reason: "Invalid JWT header encoding")
        }
        
        let header = try JSONDecoder().decode(JWTHeader.self, from: headerData)
        
        guard let kid = header.kid else {
            throw OAuthError.invalidToken(reason: "Missing key ID in token header")
        }
        
        // Find the matching public key
        guard keys.keys.contains(where: { $0.kid == kid }) else {
            throw OAuthError.invalidToken(reason: "No matching public key found")
        }
        
        // TODO: Implement proper Apple public key verification
        // Note: This is a placeholder - Apple Sign In verification typically requires
        // fetching and caching Apple's public keys from their JWKS endpoint
        // For production, use the actual Apple public key from https://appleid.apple.com/auth/keys
        // and using JWTKit to verify the signature
        
        // For now, this will fail - you need to implement JWKS key parsing and RSA verification
        throw OAuthError.invalidToken(reason: "Apple Sign In verification not fully implemented - requires JWKS key fetching and RSA verification")
    }
    
    /// Fetch Apple's public keys for JWT verification
    private func fetchPublicKeys() async throws -> ApplePublicKeysResponse {
        // Return cached keys if still valid
        if let keys = publicKeys,
           let lastFetch = lastKeyFetch,
           Date().timeIntervalSince(lastFetch) < keysCacheDuration {
            return keys
        }
        
        // Fetch new keys
        var request = HTTPClientRequest(url: publicKeysEndpoint)
        request.method = .GET
        
        let response = try await httpClient.execute(request, timeout: .seconds(30))
        
        guard response.status == .ok else {
            throw OAuthError.networkError("Failed to fetch Apple public keys")
        }
        
        let bodyBytes = try await response.body.collect(upTo: 1024 * 1024)
        let keys = try JSONDecoder().decode(ApplePublicKeysResponse.self, from: bodyBytes)
        
        // Cache the keys
        self.publicKeys = keys
        self.lastKeyFetch = Date()
        
        return keys
    }
}

/// JWT Header structure for parsing
struct JWTHeader: Codable {
    let alg: String
    let kid: String?
}

/// Extension to decode base64 URL encoded strings
extension Data {
    init?(base64URLEncoded string: String) {
        var base64 = string
            .replacingOccurrences(of: "-", with: "+")
            .replacingOccurrences(of: "_", with: "/")
        
        // Add padding if needed
        let paddingLength = (4 - base64.count % 4) % 4
        base64 += String(repeating: "=", count: paddingLength)
        
        guard let data = Data(base64Encoded: base64) else {
            return nil
        }
        
        self = data
    }
}
