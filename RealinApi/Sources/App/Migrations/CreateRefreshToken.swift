import Fluent

/// Migration to create the refresh_tokens table
struct CreateRefreshToken: AsyncMigration {
    func prepare(on database: Database) async throws {
        try await database.schema("refresh_tokens")
            .id()
            .field("user_id", .uuid, .required, .references("users", "id", onDelete: .cascade))
            .field("token", .string, .required)
            .field("expires_at", .datetime, .required)
            .field("is_revoked", .bool, .required)
            .field("device_name", .string)
            .field("ip_address", .string)
            .field("created_at", .datetime)
            .field("updated_at", .datetime)
            .unique(on: "token")
            .create()
        
        // Note: Indexes for user_id and expires_at can be added manually in production if needed
    }
    
    func revert(on database: Database) async throws {
        try await database.schema("refresh_tokens").delete()
    }
}
