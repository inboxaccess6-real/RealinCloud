import Fluent

/// Migration to create the otps table for phone OTP authentication
struct CreateOTP: AsyncMigration {
    func prepare(on database: Database) async throws {
        try await database.schema("otps")
            .id()
            .field("phone_number", .string, .required)  // Phone number in E.164 format
            .field("code", .string, .required)  // The OTP code
            .field("attempts", .int, .required)  // Number of verification attempts
            .field("ip_address", .string)  // IP address for security/audit
            .field("expires_at", .datetime, .required)  // Expiration time
            .field("created_at", .datetime)
            .field("updated_at", .datetime)
            .create()
        
        // Note: Indexes for phone_number and expires_at can be added manually in production if needed
    }
    
    func revert(on database: Database) async throws {
        try await database.schema("otps").delete()
    }
}
