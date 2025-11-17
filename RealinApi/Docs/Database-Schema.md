# Database Schema Documentation

## Overview

This document describes the PostgreSQL database schema for the RealinApi authentication system using Fluent ORM.

---

## Tables

### `users`

Stores user accounts authenticated via OAuth providers (Google and Apple).

**Schema**:

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PRIMARY KEY | Unique identifier for the user |
| oauth_provider | VARCHAR(255) | NOT NULL | OAuth provider ("google" or "apple") |
| oauth_id | VARCHAR(255) | NOT NULL | Unique user ID from OAuth provider |
| email | VARCHAR(255) | NOT NULL | User's email address |
| name | VARCHAR(255) | NULL | User's full name (optional) |
| picture_url | VARCHAR(255) | NULL | URL to user's profile picture (optional) |
| created_at | TIMESTAMP | NULL | Timestamp when user was created |
| updated_at | TIMESTAMP | NULL | Timestamp when user was last updated |

**Indexes**:

- **Primary Key**: `id`
- **Unique Index**: `(oauth_provider, oauth_id)` - Ensures one account per OAuth provider
- **Unique Index**: `email` - Ensures unique email addresses across all users

**SQL Definition**:

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    oauth_provider VARCHAR(255) NOT NULL,
    oauth_id VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    name VARCHAR(255),
    picture_url VARCHAR(255),
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    UNIQUE (oauth_provider, oauth_id),
    UNIQUE (email)
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_oauth ON users(oauth_provider, oauth_id);
```

---

### `_fluent_migrations`

Tracks database migrations that have been run. This is automatically managed by Fluent.

**Schema**:

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PRIMARY KEY | Migration record ID |
| name | VARCHAR(255) | NOT NULL UNIQUE | Migration name |
| batch | INTEGER | NOT NULL | Migration batch number |
| created_at | TIMESTAMP | NULL | When migration was run |

---

## Fluent Model Mapping

### User Model

**File**: `Sources/App/Models/User.swift`

**Swift to Database Mapping**:

```swift
final class User: Model {
    static let schema = "users"
    
    @ID(key: .id)
    var id: UUID?                          // → id (UUID)
    
    @Field(key: "oauth_provider")
    var oauthProvider: OAuthProvider       // → oauth_provider (VARCHAR)
    
    @Field(key: "oauth_id")
    var oauthId: String                    // → oauth_id (VARCHAR)
    
    @Field(key: "email")
    var email: String                      // → email (VARCHAR)
    
    @OptionalField(key: "name")
    var name: String?                      // → name (VARCHAR, NULL)
    
    @OptionalField(key: "picture_url")
    var pictureUrl: String?                // → picture_url (VARCHAR, NULL)
    
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?                   // → created_at (TIMESTAMP)
    
    @Timestamp(key: "updated_at", on: .update)
    var updatedAt: Date?                   // → updated_at (TIMESTAMP)
}
```

---

## Migrations

### Migration: `CreateUser`

**File**: `Sources/App/Migrations/CreateUser.swift`

**Purpose**: Create the `users` table with indexes and constraints.

**Code**:

```swift
struct CreateUser: AsyncMigration {
    func prepare(on database: Database) async throws {
        try await database.schema("users")
            .id()
            .field("oauth_provider", .string, .required)
            .field("oauth_id", .string, .required)
            .field("email", .string, .required)
            .field("name", .string)
            .field("picture_url", .string)
            .field("created_at", .datetime)
            .field("updated_at", .datetime)
            .unique(on: "oauth_provider", "oauth_id")
            .unique(on: "email")
            .create()
    }
    
    func revert(on database: Database) async throws {
        try await database.schema("users").delete()
    }
}
```

**When it runs**: Automatically on application startup via `fluent.migrate()`.

**Revert**: To rollback, drop the `users` table.

---

## Queries

### Common Database Operations

#### 1. Find User by OAuth Provider and ID

**Swift (Fluent)**:
```swift
let user = try await User.query(on: db)
    .filter(\.$oauthProvider == .google)
    .filter(\.$oauthId == "1234567890")
    .first()
```

**Equivalent SQL**:
```sql
SELECT * FROM users 
WHERE oauth_provider = 'google' 
  AND oauth_id = '1234567890' 
LIMIT 1;
```

#### 2. Find User by Email

**Swift (Fluent)**:
```swift
let user = try await User.query(on: db)
    .filter(\.$email == "user@example.com")
    .first()
```

**Equivalent SQL**:
```sql
SELECT * FROM users 
WHERE email = 'user@example.com' 
LIMIT 1;
```

#### 3. Create New User

**Swift (Fluent)**:
```swift
let user = User(
    oauthProvider: .google,
    oauthId: "1234567890",
    email: "user@example.com",
    name: "John Doe"
)
try await user.save(on: db)
```

**Equivalent SQL**:
```sql
INSERT INTO users (id, oauth_provider, oauth_id, email, name, created_at, updated_at)
VALUES (gen_random_uuid(), 'google', '1234567890', 'user@example.com', 'John Doe', NOW(), NOW())
RETURNING *;
```

#### 4. Update User

**Swift (Fluent)**:
```swift
user.name = "Jane Doe"
user.pictureUrl = "https://new-picture.com"
try await user.save(on: db)
```

**Equivalent SQL**:
```sql
UPDATE users 
SET name = 'Jane Doe', 
    picture_url = 'https://new-picture.com',
    updated_at = NOW()
WHERE id = 'user-uuid';
```

#### 5. Find User by ID

**Swift (Fluent)**:
```swift
let user = try await User.find(userId, on: db)
```

**Equivalent SQL**:
```sql
SELECT * FROM users 
WHERE id = 'user-uuid' 
LIMIT 1;
```

---

## Data Integrity

### Unique Constraints

#### 1. OAuth Provider + OAuth ID

**Constraint**: `UNIQUE (oauth_provider, oauth_id)`

**Purpose**: Ensures a user can have only one account per OAuth provider.

**Example Scenario**:
- User signs in with Google (oauth_provider='google', oauth_id='12345')
- User tries to sign in again with the same Google account
- System finds existing user and returns it instead of creating a duplicate

#### 2. Email

**Constraint**: `UNIQUE (email)`

**Purpose**: Ensures email addresses are unique across all users.

**Example Scenario**:
- User A signs in with Google using email@example.com
- User B tries to sign in with Apple using the same email@example.com
- System will reject or merge accounts (depending on implementation choice)

**Note**: The current implementation allows different OAuth providers to share the same email. You may want to add logic to prevent this or implement account linking.

---

## Timestamps

### Automatic Timestamp Management

Fluent automatically manages timestamps using the `@Timestamp` property wrapper:

- `created_at`: Set once when the record is created
- `updated_at`: Updated automatically on every save

**Implementation**:
```swift
@Timestamp(key: "created_at", on: .create)
var createdAt: Date?

@Timestamp(key: "updated_at", on: .update)
var updatedAt: Date?
```

---

## Database Connection

### Configuration

**File**: `Sources/App/App+build.swift`

**Connection String Format**:
```
postgres://username:password@hostname:port/database
```

**Example**:
```
postgres://realin_user:password123@localhost:5432/realin_db
```

**Environment Variable**:
```bash
export DATABASE_URL="postgres://localhost/realin_db"
```

**Swift Configuration**:
```swift
let databaseConfig = PostgresConfiguration(
    hostname: "localhost",
    port: 5432,
    username: "realin_user",
    password: "password123",
    database: "realin_db",
    tls: .disable // Use .require for production
)
```

---

## Performance Considerations

### Indexes

The current schema includes the following indexes for optimal query performance:

1. **Primary Key (id)**: Automatic B-tree index
2. **(oauth_provider, oauth_id)**: Composite unique index for OAuth lookups
3. **(email)**: Unique index for email lookups

### Query Optimization

**Fast Queries** (using indexes):
```sql
-- Uses oauth index
SELECT * FROM users WHERE oauth_provider = 'google' AND oauth_id = '12345';

-- Uses email index
SELECT * FROM users WHERE email = 'user@example.com';

-- Uses primary key
SELECT * FROM users WHERE id = 'uuid-here';
```

**Slow Queries** (no index):
```sql
-- Full table scan
SELECT * FROM users WHERE name = 'John Doe';

-- Full table scan
SELECT * FROM users WHERE picture_url LIKE '%google%';
```

### Adding Additional Indexes

If you frequently query by `name` or `created_at`, add indexes:

```sql
CREATE INDEX idx_users_name ON users(name);
CREATE INDEX idx_users_created_at ON users(created_at);
```

In Fluent migration:
```swift
.field("name", .string)
.field("created_at", .datetime)
// ... in prepare()
database.schema("users")
    // ... other fields
    .create()

// After table creation:
try await database.raw(
    "CREATE INDEX idx_users_name ON users(name)"
).run()
```

---

## Database Maintenance

### Backup

**Manual Backup**:
```bash
pg_dump realin_db > backup_$(date +%Y%m%d).sql
```

**Restore**:
```bash
psql realin_db < backup_20251115.sql
```

### Vacuum (Performance)

Regularly vacuum the database to reclaim storage and update statistics:

```sql
VACUUM ANALYZE users;
```

### Monitor Table Size

```sql
SELECT 
    pg_size_pretty(pg_total_relation_size('users')) AS total_size,
    pg_size_pretty(pg_relation_size('users')) AS table_size,
    pg_size_pretty(pg_total_relation_size('users') - pg_relation_size('users')) AS indexes_size;
```

---

## Migration Management

### View Applied Migrations

```sql
SELECT * FROM _fluent_migrations ORDER BY batch, created_at;
```

### Rollback Last Migration

**Note**: Not supported automatically. You must manually revert.

**Steps**:
1. Identify the migration to revert
2. Run the `revert()` method code manually
3. Delete from `_fluent_migrations` table

**Example**:
```sql
-- Drop the table
DROP TABLE users;

-- Remove migration record
DELETE FROM _fluent_migrations WHERE name = 'CreateUser';
```

### Add New Migration

1. Create migration file in `Sources/App/Migrations/`
2. Register in `App+build.swift`:
   ```swift
   fluent.migrations.add(CreateUser())
   fluent.migrations.add(YourNewMigration())
   ```
3. Run the app (auto-migration will execute)

---

## Future Schema Extensions

### Suggested Additions

#### 1. Refresh Tokens Table

```sql
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id)
);
```

#### 2. User Sessions Table

```sql
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    device_name VARCHAR(255),
    ip_address INET,
    user_agent TEXT,
    last_active_at TIMESTAMP,
    created_at TIMESTAMP
);
```

#### 3. Audit Log Table

```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    action VARCHAR(100) NOT NULL,
    resource VARCHAR(100),
    details JSONB,
    ip_address INET,
    created_at TIMESTAMP NOT NULL
);
```

---

## Troubleshooting

### Common Issues

#### 1. Migration Already Exists

**Error**: "relation 'users' already exists"

**Solution**: The migration has already been run. Either:
- Skip the migration (it's already applied)
- Drop the table and re-run migrations for a fresh start

#### 2. Duplicate Key Violation

**Error**: "duplicate key value violates unique constraint"

**Cause**: Trying to insert a user with an existing oauth_provider+oauth_id or email.

**Solution**: Check for existing users before creating:
```swift
if let existingUser = try await User.query(on: db)
    .filter(\.$email == email)
    .first() {
    // User exists, update or return
} else {
    // Create new user
}
```

#### 3. Connection Refused

**Error**: "connection refused" or "could not connect to server"

**Solution**: Ensure PostgreSQL is running:
```bash
brew services start postgresql@16
# or
sudo systemctl start postgresql
```

---

## Security Notes

### SQL Injection Prevention

Fluent ORM automatically prevents SQL injection by using parameterized queries. Never concatenate user input directly into queries.

**Safe** (Fluent):
```swift
try await User.query(on: db)
    .filter(\.$email == userEmail)
    .first()
```

**Unsafe** (Raw SQL with concatenation):
```swift
// DON'T DO THIS!
try await db.raw("SELECT * FROM users WHERE email = '\(userEmail)'")
```

### Data Encryption

**Password Storage**: Not applicable (OAuth-only authentication).

**Sensitive Data**: If storing sensitive data, consider:
- Column-level encryption for PII
- Database-level encryption at rest
- TLS for connections in production

---

## Best Practices

1. **Always use migrations** for schema changes (never modify directly in production)
2. **Test migrations** in development before applying to production
3. **Backup before migrations** in production environments
4. **Use transactions** for complex operations
5. **Monitor query performance** using PostgreSQL's query analyzer
6. **Keep statistics updated** with regular VACUUM ANALYZE
7. **Use connection pooling** (handled automatically by Fluent)

---

## Support

For database-related issues, check:
- PostgreSQL logs: `/usr/local/var/log/postgresql@16.log` (macOS Homebrew)
- Application logs: Check Hummingbird logger output
- Fluent documentation: https://docs.vapor.codes/fluent/overview/
