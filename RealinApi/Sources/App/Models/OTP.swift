import Fluent
import Foundation

/// OTP (One-Time Password) model for mobile-based authentication
final class OTP: Model, @unchecked Sendable {
    static let schema = "otps"
    
    // Primary key
    @ID(key: .id)
    var id: UUID?
    
    // Phone number (E.164 format recommended: +1234567890)
    @Field(key: "phone_number")
    var phoneNumber: String
    
    // The OTP code (6 digits)
    @Field(key: "code")
    var code: String
    
    // Expiration time
    @Field(key: "expires_at")
    var expiresAt: Date
    
    // Whether the OTP has been used
    @Field(key: "is_used")
    var isUsed: Bool
    
    // Number of verification attempts
    @Field(key: "attempts")
    var attempts: Int
    
    // IP address of requester
    @OptionalField(key: "ip_address")
    var ipAddress: String?
    
    // Timestamp fields
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    // Required initializer for Fluent
    init() { }
    
    /// Initialize a new OTP
    init(
        id: UUID? = nil,
        phoneNumber: String,
        code: String,
        expiresAt: Date,
        ipAddress: String? = nil
    ) {
        self.id = id
        self.phoneNumber = phoneNumber
        self.code = code
        self.expiresAt = expiresAt
        self.isUsed = false
        self.attempts = 0
        self.ipAddress = ipAddress
    }
}
