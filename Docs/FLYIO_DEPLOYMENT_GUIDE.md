# RealinAdmin — Fly.io Deployment Guide (Free Tier)

**Date:** February 19, 2026
**Stack:** Blazor WASM + ASP.NET API (.NET 10) + PostgreSQL + Cloudflare R2
**Goal:** Deploy the full stack to Fly.io's free tier for testing/development at $0/month, with Cloudflare R2 for media file storage.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│                   Fly.io                         │
│                                                  │
│  ┌─────────────────────────────────────────┐     │
│  │  realin-api (fly app)                   │     │
│  │  ┌───────────────────────────────────┐  │     │
│  │  │ ASP.NET API (.NET 10)             │  │     │
│  │  │ + Blazor WASM (static files)      │  │     │
│  │  └───────────────────────────────────┘  │     │
│  └─────────┬───────────────┬───────────────┘     │
│            │               │                      │
│  ┌─────────▼─────────┐    │                      │
│  │  realin-db         │    │                      │
│  │  (fly postgres)    │    │                      │
│  │  PostgreSQL 16     │    │                      │
│  │  256MB RAM, 1GB    │    │                      │
│  └────────────────────┘    │                      │
└────────────────────────────┼──────────────────────┘
                             │
              ┌──────────────▼──────────────┐
              │   Cloudflare R2              │
              │   realin-media bucket        │
              │   10GB free, $0 egress       │
              │   S3-compatible API          │
              └─────────────────────────────┘
```

**Approach:** Single Fly.io app serves both the API and the Blazor WASM static files. The WASM app is published and copied into the API's `wwwroot` folder during Docker build, so one container serves everything. PostgreSQL runs as a separate Fly.io Postgres cluster. Media files (property images/videos) are stored in Cloudflare R2 — no persistent volume needed on Fly.io for uploads.

---

## Prerequisites

- [Cloudflare account](https://dash.cloudflare.com/sign-up) (free, no credit card required)
- [Fly.io account](https://fly.io/app/sign-up) (free, credit card required for verification)
- [flyctl CLI](https://fly.io/docs/flyctl/install/) installed
- Docker installed locally (for building images)
- .NET 10 SDK installed locally
- Git repository with your RealinAdmin code

### Install flyctl

```bash
# macOS
brew install flyctl

# Linux
curl -L https://fly.io/install.sh | sh

# Windows
powershell -Command "iwr https://fly.io/install.ps1 -useb | iex"
```

### Authenticate

```bash
fly auth login
```

This opens a browser for authentication. After login, verify:

```bash
fly auth whoami
```

---

## Step 1: Set Up Cloudflare R2

Cloudflare R2 provides S3-compatible object storage with **zero egress fees**. The free tier includes 10GB storage, 1M write operations, and 10M read operations per month — more than enough for testing.

### 1.1 Create a Cloudflare Account

1. Go to [dash.cloudflare.com/sign-up](https://dash.cloudflare.com/sign-up) and create a free account
2. No credit card required for the free tier

### 1.2 Enable R2 and Create a Bucket

1. In the Cloudflare dashboard, click **R2 Object Storage** in the left sidebar
2. Click **Create bucket**
3. **Bucket name:** `realin-media`
4. **Location hint:** Choose the region closest to your Fly.io app (e.g., Asia Pacific if using `sin` region)
5. Click **Create bucket**

### 1.3 Create R2 API Credentials

1. In the R2 dashboard, click **Manage R2 API Tokens** (or go to R2 Overview → **API** tab)
2. Click **Create API token**
3. **Token name:** `realin-api-access`
4. **Permissions:** Object Read & Write
5. **Scope:** Apply to specific bucket → `realin-media`
6. Click **Create API Token**

**Save these values** — you'll need them later:

| Value | Example | Where to Find |
|-------|---------|---------------|
| **Access Key ID** | `a1b2c3d4e5f6...` | Shown after token creation |
| **Secret Access Key** | `x9y8z7w6v5u4...` | Shown **once** — copy immediately |
| **Account ID** | `1234567890abcdef` | R2 dashboard URL or Account Home |
| **R2 Endpoint URL** | `https://<account-id>.r2.cloudflarestorage.com` | R2 API docs / bucket settings |

> **Important:** The Secret Access Key is only shown once. Copy and save it securely.

### 1.4 Enable Public Access (Optional — for direct image URLs)

By default R2 buckets are private (access via presigned URLs only). If you want images to be publicly accessible via a direct URL (simpler for a real estate gallery):

1. Go to your `realin-media` bucket → **Settings** tab
2. Under **Public access**, click **Allow Access**
3. You can use the R2 subdomain: `https://pub-<hash>.r2.dev/` (free, auto-generated)
4. Or connect a custom domain if you have one on Cloudflare

> For testing, presigned URLs (default behavior) work fine. Public access is better for production where you want cacheable, permanent image URLs.

### 1.5 R2 Free Tier Limits

| Resource | Free Allowance |
|----------|---------------|
| Storage | 10 GB |
| Class A ops (writes) | 1,000,000 / month |
| Class B ops (reads) | 10,000,000 / month |
| Egress (bandwidth) | **Free — unlimited** |

---

## Step 2: Update S3StorageService for R2 Compatibility

Your existing `S3StorageService` uses the default AWS credential chain and `RegionEndpoint` — these don't work with Cloudflare R2. You need to update it to support a custom endpoint URL and explicit credentials.

Replace the contents of `src/RealEstate.Infrastructure/Storage/S3StorageService.cs`:

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Storage;

public class S3StorageService : IFileStorageService
{
    private readonly string _bucketName;
    private readonly AmazonS3Client _client;
    private readonly string? _publicBaseUrl;

    public S3StorageService(IConfiguration configuration)
    {
        _bucketName = configuration["Storage:S3:BucketName"]
            ?? throw new InvalidOperationException("Storage:S3:BucketName is required.");

        var serviceUrl = configuration["Storage:S3:ServiceUrl"]
            ?? throw new InvalidOperationException("Storage:S3:ServiceUrl is required.");
        var accessKey = configuration["Storage:S3:AccessKey"]
            ?? throw new InvalidOperationException("Storage:S3:AccessKey is required.");
        var secretKey = configuration["Storage:S3:SecretKey"]
            ?? throw new InvalidOperationException("Storage:S3:SecretKey is required.");

        _publicBaseUrl = configuration["Storage:S3:PublicBaseUrl"];

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true  // Required for R2, B2, MinIO, etc.
        };

        _client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<(string Bucket, string Key)> UploadAsync(
        Stream file, string fileName, string contentType, CancellationToken ct = default)
    {
        var key = $"{Guid.NewGuid()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file,
            ContentType = contentType
        };

        await _client.PutObjectAsync(request, ct);

        return (_bucketName, key);
    }

    public async Task DeleteAsync(string bucket, string key, CancellationToken ct = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        await _client.DeleteObjectAsync(request, ct);
    }

    public Task<string> GetPresignedUrlAsync(
        string bucket, string key, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        // If public base URL is configured (R2 public bucket), return direct URL
        if (!string.IsNullOrEmpty(_publicBaseUrl))
        {
            var directUrl = $"{_publicBaseUrl.TrimEnd('/')}/{key}";
            return Task.FromResult(directUrl);
        }

        // Otherwise generate a presigned URL
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1))
        };

        var url = _client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
}
```

### What Changed from the Original

| Aspect | Before (AWS-only) | After (R2-compatible) |
|--------|-------------------|----------------------|
| Config keys | `AWS:BucketName`, `AWS:Region` | `Storage:S3:BucketName`, `Storage:S3:ServiceUrl`, `Storage:S3:AccessKey`, `Storage:S3:SecretKey` |
| Credentials | Default AWS credential chain | Explicit access key + secret key |
| Endpoint | AWS region-based | Custom `ServiceURL` (works with R2, B2, MinIO) |
| Client lifecycle | Created per-request (`using var client`) | Single instance (reused) |
| Public URLs | Not supported | Optional `PublicBaseUrl` for direct URLs |
| `ForcePathStyle` | Not set | `true` (required for R2) |

> **Backward compatibility:** If you still need AWS S3 support, the same config structure works — just set `ServiceUrl` to the AWS S3 endpoint for your region (e.g., `https://s3.ap-southeast-1.amazonaws.com`).

---

## Step 3: Create the Dockerfile

Create a `Dockerfile` in the solution root (`RealinAdmin/`):

```dockerfile
# Stage 1: Build the Blazor WASM app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS wasm-build
WORKDIR /src

# Copy project files for restore
COPY src/RealEstate.Domain/RealEstate.Domain.csproj src/RealEstate.Domain/
COPY src/RealEstate.Application/RealEstate.Application.csproj src/RealEstate.Application/
COPY src/RealEstate.Admin/RealEstate.Admin.csproj src/RealEstate.Admin/

# Restore WASM project
RUN dotnet restore src/RealEstate.Admin/RealEstate.Admin.csproj

# Copy all source
COPY src/ src/

# Publish WASM app
RUN dotnet publish src/RealEstate.Admin/RealEstate.Admin.csproj \
    -c Release \
    -o /app/wasm-publish

# Stage 2: Build the API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

# Copy project files for restore
COPY src/RealEstate.Domain/RealEstate.Domain.csproj src/RealEstate.Domain/
COPY src/RealEstate.Application/RealEstate.Application.csproj src/RealEstate.Application/
COPY src/RealEstate.Infrastructure/RealEstate.Infrastructure.csproj src/RealEstate.Infrastructure/
COPY src/RealEstate.Api/RealEstate.Api.csproj src/RealEstate.Api/

# Restore API project
RUN dotnet restore src/RealEstate.Api/RealEstate.Api.csproj

# Copy all source
COPY src/ src/

# Publish API
RUN dotnet publish src/RealEstate.Api/RealEstate.Api.csproj \
    -c Release \
    -o /app/api-publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy API publish output
COPY --from=api-build /app/api-publish .

# Copy WASM static files into API's wwwroot
# Blazor WASM publishes to wwwroot/ subfolder
COPY --from=wasm-build /app/wasm-publish/wwwroot ./wwwroot

# Fly.io uses port 8080 by default
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "RealEstate.Api.dll"]
```

> **Note:** The WASM output path may vary. After `dotnet publish`, Blazor WASM files are typically in `wwwroot/` within the publish output. The `_framework` folder (containing `blazor.boot.json`, DLLs) should end up inside the API's `wwwroot/_framework/`.

---

## Step 4: Configure the API to Serve WASM Static Files

Update `Program.cs` in the API project to serve the Blazor WASM files and handle SPA routing:

```csharp
// Add after app.UseRouting() or after building the app:

// Serve static files (wwwroot — includes Blazor WASM)
app.UseStaticFiles();

// ... your API endpoint mappings (app.MapPropertyEndpoints(), etc.) ...

// SPA fallback — serve index.html for non-API, non-file routes
app.MapFallbackToFile("index.html");
```

This means:
- API routes (`/api/*`) are handled by your endpoints
- Static files (`/_framework/*`, `/css/*`, etc.) are served from wwwroot
- Media files are served directly from Cloudflare R2 (presigned or public URLs) — no local file serving needed
- Everything else falls back to `index.html` (Blazor WASM router takes over)

> **Note:** You can remove the existing `UseStaticFiles` block for `/uploads` since media is now served from R2.

---

## Step 5: Create Production App Settings

Create `src/RealEstate.Api/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "RealinAdmin",
    "Audience": "RealinAdmin",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "Storage": {
    "Provider": "s3",
    "S3": {
      "ServiceUrl": "",
      "AccessKey": "",
      "SecretKey": "",
      "BucketName": "realin-media",
      "PublicBaseUrl": ""
    }
  },
  "AllowedOrigins": [
    "https://realin-api.fly.dev"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

> **Important:** The connection string, JWT key, and R2 credentials will be set via Fly.io secrets (environment variables), not in this file. Leave sensitive values empty here — they'll be overridden by environment variables at runtime.

### How R2 Config Maps to Secrets

| appsettings Key | Fly.io Secret | Example Value |
|-----------------|---------------|---------------|
| `Storage:S3:ServiceUrl` | `Storage__S3__ServiceUrl` | `https://a1b2c3d4.r2.cloudflarestorage.com` |
| `Storage:S3:AccessKey` | `Storage__S3__AccessKey` | Your R2 Access Key ID |
| `Storage:S3:SecretKey` | `Storage__S3__SecretKey` | Your R2 Secret Access Key |
| `Storage:S3:BucketName` | (in appsettings) | `realin-media` |
| `Storage:S3:PublicBaseUrl` | `Storage__S3__PublicBaseUrl` | `https://pub-xxxx.r2.dev` (if public access enabled) |

> **Note:** .NET uses `__` (double underscore) as the hierarchy separator for environment variables. So `Storage:S3:AccessKey` in appsettings becomes `Storage__S3__AccessKey` as an environment variable.

Update the WASM app's API base URL. Create or update `src/RealEstate.Admin/wwwroot/appsettings.Production.json`:

```json
{
  "ApiBaseUrl": "https://realin-api.fly.dev"
}
```

> Replace `realin-api` with your actual Fly.io app name (chosen in Step 4).

---

## Step 6: Create the Fly.io App

```bash
cd /path/to/RealinAdmin

# Create the app (choose a unique name)
fly apps create realin-api --machines
```

Or use `fly launch` which auto-detects .NET and generates config:

```bash
fly launch --name realin-api --region sin --no-deploy
```

- `--region sin` = Singapore (choose the closest region to you)
- `--no-deploy` = just create config, don't deploy yet

### Available Free Regions

Pick the region closest to your users. Some popular options:

| Code | Location |
|------|----------|
| `sin` | Singapore |
| `bom` | Mumbai |
| `nrt` | Tokyo |
| `lax` | Los Angeles |
| `ord` | Chicago |
| `iad` | Ashburn (Virginia) |
| `lhr` | London |
| `ams` | Amsterdam |
| `syd` | Sydney |

Full list: `fly platform regions`

---

## Step 7: Create fly.toml

Create `fly.toml` in the solution root (`RealinAdmin/`):

```toml
app = "realin-api"
primary_region = "sin"

[build]
  dockerfile = "Dockerfile"

[env]
  ASPNETCORE_ENVIRONMENT = "Production"

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = "stop"
  auto_start_machines = true
  min_machines_running = 0

  [http_service.concurrency]
    type = "requests"
    hard_limit = 250
    soft_limit = 200

[[vm]]
  memory = "256mb"
  cpu_kind = "shared"
  cpus = 1
```

### Key Settings Explained

| Setting | Value | Why |
|---------|-------|-----|
| `auto_stop_machines = "stop"` | Stops VM after no traffic | Saves free-tier resources |
| `auto_start_machines = true` | Starts VM on incoming request | Auto-wakes on traffic |
| `min_machines_running = 0` | Allow scaling to zero | No cost when idle |
| `memory = "256mb"` | Smallest VM | Fits in free tier |
| `force_https = true` | HTTPS only | Free SSL from Fly.io |

> **No `[mounts]` section needed** — media files are stored in Cloudflare R2, not on local disk. This also means no persistent volume to create, saving you 1GB of free-tier storage allocation.

> **Cold starts:** When the app scales to zero and receives a new request, it takes ~5-15 seconds to start. This is fine for testing. To avoid cold starts, set `min_machines_running = 1` (uses more free-tier allocation).

---

## Step 8: Create the PostgreSQL Database

Fly.io provides managed Postgres as a separate Fly app:

```bash
fly postgres create --name realin-db --region sin
```

When prompted, select:
- **Configuration:** `Development - Single node, 1x shared CPU, 256MB RAM, 1GB disk`

This is the free tier option. Save the credentials it outputs — you'll need them.

### Attach the Database to Your App

```bash
fly postgres attach realin-db --app realin-api
```

This automatically:
1. Creates a database user for `realin-api`
2. Sets the `DATABASE_URL` secret on `realin-api`
3. Allows network access between the two apps

Verify the connection string was set:

```bash
fly secrets list --app realin-api
```

You should see `DATABASE_URL` in the list.

### Connection String Format

Fly.io sets `DATABASE_URL` in this format:
```
postgres://username:password@realin-db.flycast:5432/realin_api?sslmode=disable
```

Your API needs to read this. Update your `Program.cs` or connection string config to read from `DATABASE_URL` environment variable:

```csharp
// In Program.cs, before building the app:
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Fly.io sets DATABASE_URL — use it if available
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convert postgres:// URL to Npgsql connection string
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Disable";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

> **Note:** Internal Fly.io Postgres connections use `flycast` DNS and don't need SSL (traffic is encrypted within the private network). Use `SSL Mode=Disable` or `sslmode=disable`.

---

## Step 9: Set Secrets (Environment Variables)

Set your JWT key, R2 credentials, and any other secrets:

```bash
# JWT key (at least 32 characters)
fly secrets set JWT_KEY="your-super-secret-jwt-key-at-least-32-chars-long" --app realin-api

# Cloudflare R2 credentials (from Step 1.3)
fly secrets set \
  Storage__S3__ServiceUrl="https://<your-account-id>.r2.cloudflarestorage.com" \
  Storage__S3__AccessKey="your-r2-access-key-id" \
  Storage__S3__SecretKey="your-r2-secret-access-key" \
  --app realin-api

# If you enabled R2 public access (Step 1.4), also set the public URL:
fly secrets set Storage__S3__PublicBaseUrl="https://pub-xxxx.r2.dev" --app realin-api
```

> **Note:** Use `__` (double underscore) as the hierarchy separator. .NET automatically maps `Storage__S3__AccessKey` to `Storage:S3:AccessKey` in configuration.

Update your `Program.cs` to read the JWT key from environment:

```csharp
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key not configured");
```

View all secrets:

```bash
fly secrets list --app realin-api
```

You should see:

```
NAME                        DIGEST                  CREATED AT
DATABASE_URL                xxxxxxxxxxxxxxxx        ...
JWT_KEY                     xxxxxxxxxxxxxxxx        ...
Storage__S3__ServiceUrl     xxxxxxxxxxxxxxxx        ...
Storage__S3__AccessKey      xxxxxxxxxxxxxxxx        ...
Storage__S3__SecretKey      xxxxxxxxxxxxxxxx        ...
Storage__S3__PublicBaseUrl   xxxxxxxxxxxxxxxx        ...
```

---

## Step 10: Add .dockerignore

Create `.dockerignore` in the solution root to speed up builds:

```
**/bin/
**/obj/
**/node_modules/
**/.git/
**/uploads/
*.md
.DS_Store
.vs/
.vscode/
*.user
*.suo
```

---

## Step 11: Deploy

### First Deployment

```bash
cd /path/to/RealinAdmin

fly deploy
```

This will:
1. Build the Docker image (locally or on Fly.io's remote builder)
2. Push the image to Fly.io's registry
3. Create a machine and start your app
4. Attach the persistent volume
5. Set up HTTPS with automatic SSL certificate

Watch the deployment logs:

```bash
fly logs
```

### Run Database Migrations

After the first deploy, you need to run EF Core migrations. You have two options:

**Option A: Run migrations on startup (recommended for dev)**

Add to `Program.cs`:

```csharp
// Auto-migrate in development/staging
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
```

> For production, you'd typically use a more controlled migration strategy. But for testing on Fly.io free tier, auto-migrate on startup is fine.

**Option B: Run migrations via flyctl SSH**

```bash
# SSH into the running machine
fly ssh console --app realin-api

# Inside the container, run migrations manually
# (requires dotnet-ef tool in the Docker image — not recommended for production images)
```

**Option C: Connect to Fly Postgres directly and run SQL**

```bash
fly postgres connect --app realin-db

# Run your migration SQL manually
\c realin_api
-- paste SQL here
```

---

## Step 12: Verify Deployment

### Check App Status

```bash
fly status --app realin-api
```

### Open in Browser

```bash
fly open --app realin-api
```

Or navigate to `https://realin-api.fly.dev`

### Check Logs

```bash
# Live logs
fly logs --app realin-api

# Recent logs
fly logs --app realin-api -n 100
```

### Check Database

```bash
# Connect to Postgres
fly postgres connect --app realin-db

# List databases
\l

# Connect to your database
\c realin_api

# Check tables
\dt realin.*
```

### Test API Health

```bash
curl https://realin-api.fly.dev/api/health
```

---

## Step 13: Custom Domain (Optional)

If you want a custom domain instead of `*.fly.dev`:

```bash
# Add your domain
fly certs add yourdomain.com --app realin-api

# Get the DNS records to configure
fly certs show yourdomain.com --app realin-api
```

Then add the CNAME/A records at your DNS provider. Fly.io automatically provisions and renews SSL certificates via Let's Encrypt.

---

## Step 14: Subsequent Deployments

After code changes, deploy with:

```bash
fly deploy
```

### Zero-Downtime Deploy

Fly.io does rolling deployments by default. The new machine starts, health checks pass, then the old machine stops.

### Deploy with Build Args

```bash
fly deploy --build-arg SOME_VAR=value
```

---

## Step 15: Monitoring & Maintenance

### Check Resource Usage

```bash
# Machine status
fly status --app realin-api

# Postgres status
fly status --app realin-db

# R2 storage usage — check via Cloudflare dashboard
# Dashboard → R2 → realin-media → Metrics tab
```

### Scale Up (When Needed)

```bash
# More memory (leaves free tier)
fly scale memory 512 --app realin-api

# More CPU
fly scale vm shared-cpu-2x --app realin-api
```

### Backup Postgres

```bash
# List snapshots (Fly takes daily snapshots automatically)
fly postgres backup list --app realin-db

# Manual backup via pg_dump
fly proxy 15432:5432 --app realin-db &
pg_dump -h localhost -p 15432 -U postgres realin_api > backup.sql
```

### Restart App

```bash
fly apps restart realin-api
```

---

## Free Tier Limits Summary

### Fly.io

| Resource | Free Allowance | Your Usage |
|----------|---------------|------------|
| **Shared CPU VMs** | Up to 3 shared-cpu-1x VMs | 1 (API) + 1 (Postgres) = 2 |
| **Memory** | 256MB per VM | 256MB API + 256MB Postgres |
| **Persistent Storage** | 3GB total | 1GB (Postgres only) |
| **Bandwidth** | 100GB outbound/month | Well within limits for testing |
| **SSL/TLS** | Free, auto-renewed | Included |
| **IPv4** | Shared (free) | Included |

### Cloudflare R2

| Resource | Free Allowance | Your Usage |
|----------|---------------|------------|
| **Storage** | 10 GB | Property images + videos |
| **Class A ops (writes)** | 1,000,000 / month | Image uploads |
| **Class B ops (reads)** | 10,000,000 / month | Image views |
| **Egress (bandwidth)** | Unlimited — $0 | All image serving |

### Combined Total: $0/month

---

## Troubleshooting

### App Won't Start

```bash
# Check logs for errors
fly logs --app realin-api -n 200

# SSH into the machine for debugging
fly ssh console --app realin-api

# Check if the app binary is there
ls -la /app/
```

### Database Connection Refused

```bash
# Verify DATABASE_URL is set
fly secrets list --app realin-api

# Check Postgres is running
fly status --app realin-db

# Test connection from app machine
fly ssh console --app realin-api
apt-get update && apt-get install -y postgresql-client
psql "$DATABASE_URL"
```

### Cold Start Too Slow

If the ~10-15 second cold start is annoying:

```toml
# In fly.toml, keep one machine always running:
[http_service]
  min_machines_running = 1
```

> This uses more of your free-tier allocation but eliminates cold starts.

### CORS Issues

If the WASM app and API are served from the same origin (`realin-api.fly.dev`), you shouldn't need CORS at all. Remove or update CORS config:

```csharp
// Since WASM and API are same origin, CORS is not needed
// But keep it for development (different ports locally)
if (app.Environment.IsDevelopment())
{
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}
```

### R2 Upload Fails / Images Not Loading

```bash
# Check logs for S3/R2 errors
fly logs --app realin-api -n 200 | grep -i "s3\|storage\|r2\|upload"

# Verify R2 secrets are set correctly
fly secrets list --app realin-api

# Test R2 connectivity from inside the container
fly ssh console --app realin-api
curl -I https://<account-id>.r2.cloudflarestorage.com
```

Common causes:
- **Wrong ServiceUrl** — must be `https://<account-id>.r2.cloudflarestorage.com` (no bucket name in URL)
- **Wrong credentials** — regenerate the R2 API token and update secrets
- **Bucket doesn't exist** — verify bucket name matches `Storage:S3:BucketName`
- **`ForcePathStyle` not set** — must be `true` for R2 (handled in updated S3StorageService)

---

## Quick Reference Commands

```bash
# Deploy
fly deploy

# View logs
fly logs --app realin-api

# SSH into app
fly ssh console --app realin-api

# Open in browser
fly open --app realin-api

# Check status
fly status --app realin-api

# Connect to database
fly postgres connect --app realin-db

# Set secrets (example: update R2 credentials)
fly secrets set Storage__S3__AccessKey="new-key" --app realin-api

# List secrets
fly secrets list --app realin-api

# Restart
fly apps restart realin-api

# Scale
fly scale memory 512 --app realin-api

# Destroy (careful!)
fly apps destroy realin-api
fly apps destroy realin-db
```

---

## CI/CD with GitHub Actions (Optional)

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Fly.io

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: superfly/flyctl-actions/setup-flyctl@master

      - run: flyctl deploy --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

Generate a deploy token:

```bash
fly tokens create deploy --app realin-api
```

Add `FLY_API_TOKEN` to your GitHub repository secrets (Settings → Secrets → Actions).

---

## Next Steps After Deployment

1. **Test the full flow:** Register → Login → Create Property → Upload Images → Submit for Approval
2. **Verify R2 uploads:** Upload a property image → confirm it's stored in R2 (check Cloudflare dashboard → R2 → `realin-media` bucket)
3. **Monitor logs** for any errors: `fly logs --app realin-api`
4. **Set up backups** if storing important test data
5. **Add a health check endpoint** (`/api/health`) for Fly.io's health monitoring
6. **Enable R2 public access** when ready for production (faster image loading, no presigned URL overhead)

---

## Sources

- [Fly.io .NET Documentation](https://fly.io/docs/languages-and-frameworks/dotnet/)
- [Fly.io Postgres Documentation](https://fly.io/docs/postgres/)
- [Fly.io fly.toml Reference](https://fly.io/docs/reference/configuration/)
- [Fly.io Pricing](https://fly.io/docs/about/pricing/)
- [Cloudflare R2 Documentation](https://developers.cloudflare.com/r2/)
- [Cloudflare R2 S3 API Compatibility](https://developers.cloudflare.com/r2/api/s3/)
- [Cloudflare R2 Pricing](https://developers.cloudflare.com/r2/pricing/)
- [Deploy ASP.NET to Fly.io — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
