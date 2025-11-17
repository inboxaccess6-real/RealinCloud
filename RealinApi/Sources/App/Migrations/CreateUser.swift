import Fluent

/// Migration to create the users table
struct CreateUser: AsyncMigration {
    func prepare(on database: Database) async throws {
        try await database.schema("users")
            .id()
            .field("oauth_provider", .string)
            .field("oauth_id", .string)
            .field("email", .string)  // Make email optional (not all users will have email)
            .field("phone_number", .string)  // Add phone number field
            .field("name", .string)
            .field("picture_url", .string)
            .field("created_at", .datetime)
            .field("updated_at", .datetime)
            .create()
        
        // Note: Partial unique indexes with WHERE clauses are not supported by Fluent schema builder
        // These would need to be added manually in production using raw SQL if needed
        // For now, uniqueness will be enforced at the application level
    }
    
    func revert(on database: Database) async throws {
        try await database.schema("users").delete()
    }
}
