#!/bin/bash

# RealinApi Setup Script

echo "🚀 Setting up RealinApi..."

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 10 SDK"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build project
echo "🔨 Building project..."
dotnet build

# Check if PostgreSQL is running
echo "🔍 Checking PostgreSQL..."
if command -v psql &> /dev/null; then
    echo "✅ PostgreSQL CLI found"
else
    echo "⚠️  PostgreSQL CLI not found. Make sure PostgreSQL is installed and running"
fi

# Create database (optional - requires psql)
read -p "Do you want to create the database now? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    read -p "Enter database name (default: realin_db): " DB_NAME
    DB_NAME=${DB_NAME:-realin_db}
    
    read -p "Enter PostgreSQL username (default: postgres): " DB_USER
    DB_USER=${DB_USER:-postgres}
    
    echo "Creating database $DB_NAME..."
    createdb -U $DB_USER $DB_NAME 2>/dev/null || echo "Database might already exist"
    
    # Run migrations
    echo "🔄 Running database migrations..."
    dotnet ef database update
    
    echo "✅ Database setup complete!"
fi

echo ""
echo "📋 Next steps:"
echo "1. Update appsettings.json with your database connection string"
echo "2. Configure OAuth providers (Google, Apple) in appsettings.json"
echo "3. Set a strong JWT secret key"
echo "4. Run: dotnet run"
echo "5. Access Swagger UI at: https://localhost:5001/swagger"
echo ""
echo "🎉 Setup complete!"
