import Foundation
import Fluent

/// Service for managing refresh tokens
actor RefreshTokenService {
    // Refresh token expiration: 90 days (configurable)
    private let refreshTokenExpirationTime: TimeInterval
    
    init(refreshTokenExpirationTime: TimeInterval = 90 * 24 * 60 * 60) {
        self.refreshTokenExpirationTime = refreshTokenExpirationTime
    }
    
    /// Generate a new refresh token for a user
    /// - Parameters:
    ///   - userId: The user's UUID
    ///   - deviceName: Optional device name
    ///   - ipAddress: Optional IP address
    ///   - db: Database connection
    /// - Returns: The refresh token string
    func generateRefreshToken(
        userId: UUID,
        deviceName: String? = nil,
        ipAddress: String? = nil,
        on db: Database
    ) async throws -> String {
        // Generate a secure random token
        let tokenString = generateSecureToken()
        
        // Calculate expiration date
        let expiresAt = Date().addingTimeInterval(refreshTokenExpirationTime)
        
        // Create refresh token record
        let refreshToken = RefreshToken(
            userId: userId,
            token: tokenString,
            expiresAt: expiresAt,
            deviceName: deviceName,
            ipAddress: ipAddress
        )
        
        try await refreshToken.save(on: db)
        
        return tokenString
    }
    
    /// Verify a refresh token and return the associated user ID
    /// - Parameters:
    ///   - token: The refresh token string
    ///   - db: Database connection
    /// - Returns: The user ID if token is valid
    /// - Throws: Error if token is invalid, expired, or revoked
    func verifyRefreshToken(_ token: String, on db: Database) async throws -> UUID {
        // Find the refresh token
        guard let refreshToken = try await RefreshToken.query(on: db)
            .filter(\.$token == token)
            .first() else {
            throw RefreshTokenError.invalidToken
        }
        
        // Check if token is revoked
        guard !refreshToken.isRevoked else {
            throw RefreshTokenError.tokenRevoked
        }
        
        // Check if token is expired
        guard refreshToken.expiresAt > Date() else {
            throw RefreshTokenError.tokenExpired
        }
        
        return refreshToken.$user.id
    }
    
    /// Revoke a specific refresh token
    /// - Parameters:
    ///   - token: The refresh token string
    ///   - db: Database connection
    func revokeRefreshToken(_ token: String, on db: Database) async throws {
        guard let refreshToken = try await RefreshToken.query(on: db)
            .filter(\.$token == token)
            .first() else {
            throw RefreshTokenError.invalidToken
        }
        
        refreshToken.isRevoked = true
        try await refreshToken.save(on: db)
    }
    
    /// Revoke all refresh tokens for a user (useful for logout from all devices)
    /// - Parameters:
    ///   - userId: The user's UUID
    ///   - db: Database connection
    func revokeAllUserTokens(userId: UUID, on db: Database) async throws {
        try await RefreshToken.query(on: db)
            .filter(\.$user.$id == userId)
            .set(\.$isRevoked, to: true)
            .update()
    }
    
    /// Delete expired refresh tokens (cleanup task)
    /// - Parameter db: Database connection
    func deleteExpiredTokens(on db: Database) async throws {
        try await RefreshToken.query(on: db)
            .filter(\.$expiresAt < Date())
            .delete()
    }
    
    /// Generate a secure random token string
    private func generateSecureToken() -> String {
        let bytes = (0..<32).map { _ in UInt8.random(in: 0...255) }
        return Data(bytes).base64EncodedString()
            .replacingOccurrences(of: "+", with: "-")
            .replacingOccurrences(of: "/", with: "_")
            .replacingOccurrences(of: "=", with: "")
    }
}

/// Refresh token errors
enum RefreshTokenError: Error, CustomStringConvertible {
    case invalidToken
    case tokenExpired
    case tokenRevoked
    
    var description: String {
        switch self {
        case .invalidToken:
            return "Invalid refresh token"
        case .tokenExpired:
            return "Refresh token has expired"
        case .tokenRevoked:
            return "Refresh token has been revoked"
        }
    }
}
