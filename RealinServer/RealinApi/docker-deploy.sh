#!/bin/bash

# RealinApi Docker Quick Start Script

set -e

echo "🐳 RealinApi Docker Deployment"
echo "================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

echo "✅ Docker found: $(docker --version)"
echo ""

# Menu
echo "Select deployment option:"
echo "1) Build and run with Docker Compose (Development)"
echo "2) Build and run with Docker Compose (Production)"
echo "3) Build Docker image only"
echo "4) Stop all containers"
echo "5) Clean up (remove containers and volumes)"
echo ""

read -p "Enter option (1-5): " option

case $option in
    1)
        echo ""
        echo "🚀 Starting Development Environment..."
        docker-compose -f docker-compose.dev.yml up -d --build
        echo ""
        echo "✅ Services started!"
        echo "📍 API: http://localhost:8080"
        echo "📍 Swagger: http://localhost:8080/swagger"
        echo "📍 Health: http://localhost:8080/health"
        echo "📍 PostgreSQL: localhost:5432"
        echo ""
        echo "View logs: docker-compose -f docker-compose.dev.yml logs -f"
        ;;
    2)
        echo ""
        echo "🚀 Starting Production Environment..."
        
        # Check for required env vars
        if [ -z "$GOOGLE_CLIENT_ID" ] || [ -z "$APPLE_CLIENT_ID" ]; then
            echo "⚠️  Warning: OAuth environment variables not set"
            echo "Set GOOGLE_CLIENT_ID and APPLE_CLIENT_ID before deploying to production"
            echo ""
            read -p "Continue anyway? (y/n) " confirm
            if [ "$confirm" != "y" ]; then
                exit 0
            fi
        fi
        
        docker-compose up -d --build
        echo ""
        echo "✅ Services started!"
        echo "📍 API: http://localhost:8080"
        echo "📍 Health: http://localhost:8080/health"
        echo ""
        echo "View logs: docker-compose logs -f"
        ;;
    3)
        echo ""
        echo "🔨 Building Docker image..."
        docker build -t realinapi:latest .
        echo ""
        echo "✅ Image built successfully!"
        echo "📦 Size: $(docker images realinapi:latest --format "{{.Size}}")"
        echo ""
        echo "Run with: docker run -d -p 8080:8080 realinapi:latest"
        ;;
    4)
        echo ""
        echo "🛑 Stopping containers..."
        docker-compose -f docker-compose.dev.yml down 2>/dev/null || true
        docker-compose down 2>/dev/null || true
        echo "✅ Containers stopped"
        ;;
    5)
        echo ""
        echo "⚠️  This will remove all containers and database volumes!"
        read -p "Are you sure? (y/n) " confirm
        if [ "$confirm" = "y" ]; then
            echo "🧹 Cleaning up..."
            docker-compose -f docker-compose.dev.yml down -v 2>/dev/null || true
            docker-compose down -v 2>/dev/null || true
            docker rmi realinapi:test realinapi:latest 2>/dev/null || true
            echo "✅ Cleanup complete"
        else
            echo "Cancelled"
        fi
        ;;
    *)
        echo "Invalid option"
        exit 1
        ;;
esac

echo ""
