import Fluent
import Foundation

/// Refresh token model for long-lived authentication sessions
final class RefreshToken: Model, @unchecked Sendable {
    static let schema = "refresh_tokens"
    
    // Primary key
    @ID(key: .id)
    var id: UUID?
    
    // Reference to the user
    @Parent(key: "user_id")
    var user: User
    
    // The actual refresh token string
    @Field(key: "token")
    var token: String
    
    // Expiration date
    @Field(key: "expires_at")
    var expiresAt: Date
    
    // Whether the token has been revoked
    @Field(key: "is_revoked")
    var isRevoked: Bool
    
    // Device information (optional, for tracking)
    @OptionalField(key: "device_name")
    var deviceName: String?
    
    @OptionalField(key: "ip_address")
    var ipAddress: String?
    
    // Timestamp fields
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    @Timestamp(key: "updated_at", on: .update)
    var updatedAt: Date?
    
    // Required initializer for Fluent
    init() { }
    
    /// Initialize a new refresh token
    init(
        id: UUID? = nil,
        userId: UUID,
        token: String,
        expiresAt: Date,
        deviceName: String? = nil,
        ipAddress: String? = nil
    ) {
        self.id = id
        self.$user.id = userId
        self.token = token
        self.expiresAt = expiresAt
        self.isRevoked = false
        self.deviceName = deviceName
        self.ipAddress = ipAddress
    }
}
