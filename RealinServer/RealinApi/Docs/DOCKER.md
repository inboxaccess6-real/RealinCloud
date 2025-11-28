# Docker Deployment Guide

## 🐳 Docker Images

Two Dockerfile options provided:

1. **Dockerfile** (Alpine-based) - **Recommended**
   - Base: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
   - Size: ~110 MB
   - Smallest possible .NET 10 image

2. **Dockerfile.debian** (Debian Slim)
   - Base: `mcr.microsoft.com/dotnet/aspnet:10.0-bookworm-slim`
   - Size: ~220 MB
   - Better compatibility, slightly larger

## 🚀 Quick Start

### Option 1: Docker Compose (Easiest)

```bash
# Development
docker-compose -f docker-compose.dev.yml up -d

# Production
docker-compose up -d
```

This starts:
- RealinApi container on port 8080
- PostgreSQL 16 container on port 5432
- Automatic health checks
- Persistent database volume

### Option 2: Docker Build & Run Manually

```bash
# Build Alpine image
docker build -t realinapi:latest .

# Or build Debian image
docker build -f Dockerfile.debian -t realinapi:latest .

# Run container
docker run -d \
  --name realinapi \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=realin_db;Username=realin_user;Password=realin_pass" \
  -e Jwt__SecretKey="your_secret_key_min_32_chars" \
  realinapi:latest
```

## 📦 Image Size Comparison

```
Alpine:       ~110 MB  ⭐ Recommended
Debian Slim:  ~220 MB
Debian Full:  ~450 MB (not included)
```

## 🔧 Configuration

### Environment Variables

Set these in `docker-compose.yml` or via `-e` flag:

```yaml
environment:
  # Database
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=realin_db;Username=user;Password=pass
  
  # JWT
  - Jwt__SecretKey=your_32+_char_secret
  - Jwt__Issuer=RealinApi
  - Jwt__Audience=RealinApp
  - Jwt__AccessTokenExpirationMinutes=60
  
  # OAuth
  - OAuth__Google__ClientId=your_google_client_id
  - OAuth__Apple__ClientId=your_apple_client_id
  
  # OTP
  - Otp__ExpirationMinutes=10
  - Otp__MaxAttemptsPerWindow=5
  - Otp__RateLimitWindowMinutes=60
```

### Using .env File

Create `.env` file:

```bash
GOOGLE_CLIENT_ID=your_google_client_id
APPLE_CLIENT_ID=your_apple_client_id
POSTGRES_PASSWORD=secure_password
JWT_SECRET=your_secure_secret_key_32_chars_minimum
```

Then run:
```bash
docker-compose --env-file .env up -d
```

## 🗄️ Database Migrations

### Run migrations on container startup

```bash
# Exec into running container
docker exec -it realinapi sh

# Run migrations
dotnet ef database update

# Or use SQL script (recommended for production)
docker cp ./Migrations realinapi:/app/Migrations
docker exec realinapi dotnet ef database update
```

### Pre-build migrations into image

Add to Dockerfile before ENTRYPOINT:

```dockerfile
# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Run migrations on startup (add to entrypoint script)
```

## 🏗️ Multi-Stage Build Explained

```dockerfile
# Stage 1: Build (SDK image ~700MB)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
# Compile and publish app

# Stage 2: Runtime (ASP.NET image ~110MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
# Copy only published output
# Final image: ~110MB
```

## 🔒 Security Features

✅ Non-root user (appuser:1000)  
✅ Read-only filesystem (where possible)  
✅ Health checks enabled  
✅ Minimal attack surface (Alpine)  
✅ No dev dependencies in production  
✅ .dockerignore for secrets  

## 📊 Health Checks

Built-in health endpoint: `http://localhost:8080/health`

Docker automatically monitors:
- Interval: 30s
- Timeout: 3s
- Retries: 3
- Start period: 10s (allows app warmup)

## 🚢 Deployment Commands

### Development

```bash
# Start services
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f realinapi

# Stop services
docker-compose -f docker-compose.dev.yml down

# Rebuild after code changes
docker-compose -f docker-compose.dev.yml up -d --build
```

### Production

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services (keep data)
docker-compose down

# Stop and remove volumes (⚠️ deletes database)
docker-compose down -v

# Scale API containers
docker-compose up -d --scale realinapi=3
```

## 🔍 Debugging

```bash
# Check container status
docker ps

# View container logs
docker logs realinapi -f

# Exec into container
docker exec -it realinapi sh

# Check health
curl http://localhost:8080/health

# Check database connection
docker exec -it realin-postgres psql -U realin_user -d realin_db

# View container resource usage
docker stats realinapi
```

## 📈 Performance Optimization

### Build optimizations (already included):

```dockerfile
# Layer caching - copy csproj first
COPY RealinApi.csproj .
RUN dotnet restore

# Then copy source (changes more often)
COPY . .
RUN dotnet publish
```

### Runtime optimizations:

```dockerfile
# Self-contained: false (uses shared runtime)
# PublishTrimmed: false (for Alpine compatibility)
# ReadyToRun: true (optional, larger image, faster startup)
```

## 🌐 Networking

### Access from host:
- API: http://localhost:8080
- Database: localhost:5432

### Container-to-container:
- API → Database: `Host=postgres` (service name)
- Network: `realin-network` (bridge driver)

## 💾 Persistent Data

Volumes created:
```bash
# Production
postgres-data         # /var/lib/postgresql/data

# Development  
postgres-dev-data     # /var/lib/postgresql/data
```

Backup database:
```bash
docker exec realin-postgres pg_dump -U realin_user realin_db > backup.sql

# Restore
docker exec -i realin-postgres psql -U realin_user realin_db < backup.sql
```

## 🚀 Production Deployment

### AWS ECS / Azure Container Apps / GCP Cloud Run

```bash
# Tag image
docker tag realinapi:latest your-registry/realinapi:v1.0.0

# Push to registry
docker push your-registry/realinapi:v1.0.0
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: realinapi
spec:
  replicas: 3
  selector:
    matchLabels:
      app: realinapi
  template:
    metadata:
      labels:
        app: realinapi
    spec:
      containers:
      - name: realinapi
        image: your-registry/realinapi:v1.0.0
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: realinapi-secrets
              key: db-connection
```

## 🧪 Testing

```bash
# Test build
docker build -t realinapi:test .

# Test run
docker run --rm -p 8080:8080 realinapi:test

# Test in another terminal
curl http://localhost:8080/health
curl -X POST http://localhost:8080/api/auth/otp/request \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","method":"email"}'
```

## 📝 Image Size Breakdown

```
Alpine Image Layers:
- Base Alpine Linux:        ~7 MB
- .NET ASP.NET Runtime:     ~85 MB
- ICU libraries:            ~10 MB
- Your application:         ~8 MB
Total:                      ~110 MB

Debian Slim Layers:
- Base Debian:              ~80 MB
- .NET ASP.NET Runtime:     ~130 MB
- Your application:         ~10 MB
Total:                      ~220 MB
```

## 🔧 Troubleshooting

**Container won't start:**
```bash
docker logs realinapi
# Check for database connection errors
```

**Database connection refused:**
```bash
# Check if postgres is ready
docker exec realin-postgres pg_isready

# Check network
docker network inspect realin-network
```

**Image too large:**
```bash
# Use Alpine (Dockerfile)
# Enable trimming (be careful with reflection)
# Remove unnecessary files in .dockerignore
```

**Permission denied:**
```bash
# Running as non-root user (appuser)
# Check file permissions in COPY commands
```

## 📚 References

- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet-aspnet/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Alpine Linux](https://alpinelinux.org/)
