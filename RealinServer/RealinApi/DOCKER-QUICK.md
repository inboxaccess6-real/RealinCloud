# 🐳 Docker Quick Reference

## Image Size: ~230 MB (Alpine-based)

## Quick Commands

```bash
# Build image
docker build -t realinapi:latest .

# Run container
docker run -d -p 8080:8080 realinapi:latest

# Development (with PostgreSQL)
docker-compose -f docker-compose.dev.yml up -d

# Production
docker-compose up -d

# View logs
docker logs -f realinapi

# Stop
docker-compose down

# Clean up
docker-compose down -v
```

## One-Line Deploy

```bash
# Development
./docker-deploy.sh

# Or manual
docker-compose -f docker-compose.dev.yml up -d && \
  echo "API: http://localhost:8080" && \
  echo "Swagger: http://localhost:8080/swagger"
```

## Environment Variables

```bash
# Required for production
export ConnectionStrings__DefaultConnection="Host=postgres;..."
export Jwt__SecretKey="your_32+_char_secret"
export OAuth__Google__ClientId="your_google_id"
export OAuth__Apple__ClientId="your_apple_id"
```

## Health Check

```bash
curl http://localhost:8080/health
```

## Image Comparison

| Base Image | Size | Compatibility |
|------------|------|---------------|
| Alpine (default) | ~230 MB | ⭐ Best |
| Debian Slim | ~340 MB | Good |

## Container Access

```bash
# Shell access
docker exec -it realinapi sh

# View environment
docker exec realinapi env

# Database access
docker exec -it realin-postgres psql -U realin_user -d realin_db
```

## Troubleshooting

```bash
# Check container status
docker ps

# View logs
docker logs realinapi

# Inspect container
docker inspect realinapi

# Check resource usage
docker stats realinapi
```

## Production Checklist

- [ ] Set strong JWT secret (32+ chars)
- [ ] Configure OAuth client IDs
- [ ] Set database credentials
- [ ] Run migrations: `docker exec realinapi dotnet ef database update`
- [ ] Test health endpoint
- [ ] Configure reverse proxy (nginx/traefik)
- [ ] Enable HTTPS
- [ ] Set up monitoring
- [ ] Configure backups
