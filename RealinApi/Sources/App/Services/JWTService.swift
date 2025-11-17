import Foundation
@preconcurrency import JWTKit

/// JWT payload for authentication tokens
struct AuthPayload: JWTPayload, @unchecked Sendable {
    let userId: UUID
    let email: String
    let exp: ExpirationClaim
    let iat: IssuedAtClaim
    
    func verify(using signer: JWTSigner) throws {
        try exp.verifyNotExpired()
    }
}

/// Service for generating and verifying JWT tokens for authenticated users
actor JWTService {
    private let signers: JWTSigners
    private let tokenExpirationTime: TimeInterval // in seconds
    
    init(secret: String, tokenExpirationTime: TimeInterval = 30 * 60) throws { // 30 minutes default
        self.signers = JWTSigners()
        self.tokenExpirationTime = tokenExpirationTime
        
        // Add HMAC SHA-256 signer with the secret key
        let secretData = secret.data(using: .utf8) ?? Data()
        self.signers.use(.hs256(key: secretData))
    }
    
    /// Generate a JWT token for a user
    /// - Parameters:
    ///   - userId: The user's UUID
    ///   - email: The user's email
    /// - Returns: A signed JWT token string
    func generateToken(userId: UUID, email: String) throws -> String {
        let now = Date()
        let payload = AuthPayload(  
            userId: userId,
            email: email,
            exp: ExpirationClaim(value: now.addingTimeInterval(tokenExpirationTime)),
            iat: IssuedAtClaim(value: now)
        )
        
        return try signers.sign(payload)
    }
    
    /// Verify a JWT token and extract the payload
    /// - Parameter token: The JWT token to verify
    /// - Returns: The decoded AuthPayload
    func verifyToken(_ token: String) throws -> AuthPayload {
        return try signers.verify(token, as: AuthPayload.self)
    }
}

/// Extension to help with token extraction from Authorization header
extension String {
    /// Extract bearer token from "Bearer <token>" format
    var bearerToken: String? {
        let prefix = "Bearer "
        guard self.hasPrefix(prefix) else {
            return nil
        }
        return String(self.dropFirst(prefix.count))
    }
}
