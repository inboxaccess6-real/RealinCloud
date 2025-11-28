#!/bin/bash

# Setup script for Realin PostgreSQL database
# This creates the database, user, and schema for development

echo "🚀 Setting up Realin PostgreSQL Database..."

# Check if Docker container is running
if ! docker ps | grep -q my_postgres; then
    echo "❌ Error: PostgreSQL container 'my_postgres' is not running"
    echo "   Start it with: docker start my_postgres"
    exit 1
fi

echo "✅ PostgreSQL container is running"

# Create database and user
echo "📦 Creating database and user..."
docker exec -i my_postgres psql -U postgres << 'EOF'
-- Create user if not exists
DO
$$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_user WHERE usename = 'realin') THEN
      CREATE USER realin WITH PASSWORD 'Re@lter_8206';
   ELSE
      ALTER USER realin WITH PASSWORD 'Re@lter_8206';
   END IF;
END
$$;

-- Drop database if exists (for clean setup)
DROP DATABASE IF EXISTS realin_dev;

-- Create database
CREATE DATABASE realin_dev OWNER realin;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE realin_dev TO realin;
EOF

echo "✅ Database created"

# Create the realin schema
echo "📂 Creating 'realin' schema..."
docker exec -i my_postgres psql -U realin -d realin_dev << 'EOF'
-- Create the realin schema
CREATE SCHEMA IF NOT EXISTS realin;

-- Grant privileges on schema
GRANT ALL ON SCHEMA realin TO realin;

-- Set search path default for this user
ALTER USER realin SET search_path TO realin, public;

SELECT 'Schema "realin" created successfully!' as status;
EOF

echo "✅ Schema created"

# Apply EF Core migrations
echo "📋 Applying Entity Framework migrations..."
dotnet ef database update

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ =========================================="
    echo "✅  Setup completed successfully!"
    echo "✅ =========================================="
    echo ""
    echo "Database: realin_dev"
    echo "Schema:   realin"
    echo "User:     realin"
    echo ""
    echo "Connection string:"
    echo "Host=localhost;Port=5432;Database=realin_dev;Username=realin;Password=re@lter_8206;SearchPath=realin"
    echo ""
    echo "Next steps:"
    echo "1. Run: dotnet run"
    echo "2. Permissions will auto-seed on first startup"
    echo ""
else
    echo ""
    echo "❌ Migration failed. Check the error above."
    exit 1
fi
