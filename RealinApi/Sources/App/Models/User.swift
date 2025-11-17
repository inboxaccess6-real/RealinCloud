import Fluent
import Foundation

/// OAuth Provider enum to distinguish between Google, Apple, and Phone sign-in
enum OAuthProvider: String, Codable {
    case google
    case apple
    case phone
}

/// User model representing an authenticated user in the system
final class User: Model, @unchecked Sendable {
    static let schema = "users"
    
    // Primary key
    @ID(key: .id)
    var id: UUID?
    
    // OAuth provider (google, apple, or email)
    @OptionalField(key: "oauth_provider")
    var oauthProvider: OAuthProvider?
    
    // OAuth provider's unique user ID (e.g., Google user ID or Apple user ID)
    // Not required for phone authentication
    @OptionalField(key: "oauth_id")
    var oauthId: String?
    
    // User's email address (optional - only for OAuth users)
    @OptionalField(key: "email")
    var email: String?
    
    // User's phone number (optional, required for phone authentication)
    @OptionalField(key: "phone_number")
    var phoneNumber: String?
    
    // User's full name (optional)
    @OptionalField(key: "name")
    var name: String?
    
    // Profile picture URL (optional)
    @OptionalField(key: "picture_url")
    var pictureUrl: String?
    
    // Timestamp fields
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    @Timestamp(key: "updated_at", on: .update)
    var updatedAt: Date?
    
    // Required initializer for Fluent
    init() { }
    
    /// Initialize a new user
    init(
        id: UUID? = nil,
        oauthProvider: OAuthProvider?,
        oauthId: String?,
        email: String?,
        phoneNumber: String? = nil,
        name: String? = nil,
        pictureUrl: String? = nil
    ) {
        self.id = id
        self.oauthProvider = oauthProvider
        self.oauthId = oauthId
        self.email = email
        self.phoneNumber = phoneNumber
        self.name = name
        self.pictureUrl = pictureUrl
    }
    
    /// Convenience initializer for phone-only users
    convenience init(phoneNumber: String, name: String? = nil) {
        self.init(
            oauthProvider: .phone,
            oauthId: nil,
            email: nil,  // Email is nil for phone-only users
            phoneNumber: phoneNumber,
            name: name
        )
    }
}

/// User response DTO (Data Transfer Object) for API responses
struct UserResponse: Codable {
    let id: UUID
    let email: String?
    let phoneNumber: String?
    let name: String?
    let pictureUrl: String?
    let oauthProvider: String
    let createdAt: Date?
    let updatedAt: Date?
    
    init(from user: User) throws {
        guard let id = user.id else {
            throw Abort(.internalServerError, reason: "User ID is missing")
        }
        self.id = id
        self.email = user.email
        self.phoneNumber = user.phoneNumber
        self.name = user.name
        self.pictureUrl = user.pictureUrl
        self.oauthProvider = user.oauthProvider?.rawValue ?? "unknown"
        self.createdAt = user.createdAt
        self.updatedAt = user.updatedAt
    }
}

/// Custom error type for better error handling
struct Abort: Error {
    let status: HTTPResponseStatus
    let reason: String
    
    init(_ status: HTTPResponseStatus, reason: String) {
        self.status = status
        self.reason = reason
    }
}

/// HTTP Response Status enum
enum HTTPResponseStatus {
    case internalServerError
    case unauthorized
    case badRequest
    case notFound
}
