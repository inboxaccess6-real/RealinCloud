# Dev Deployment Guide — Cloudflare + AWS EC2

Architecture: Cloudflare Pages (WASM) + Cloudflare R2 (storage) + Cloudflare Tunnel → AWS EC2 (API + PostgreSQL).

**Total cost: ~$2.40/month** (just EBS storage, covered by credits).

```
┌─────────────────────────────────────────────────────────────┐
│  Cloudflare (Free Tier)                                     │
│                                                             │
│  ┌──────────────┐   ┌──────────────┐   ┌────────────────┐  │
│  │ Pages        │   │ R2 Bucket    │   │ DNS            │  │
│  │ Blazor WASM  │   │ Media files  │   │ realin.com     │  │
│  └──────┬───────┘   └──────────────┘   └────────┬───────┘  │
│         │                                       │           │
│         │  API calls to api.realin.com           │           │
│         └───────────────────────────────┐       │           │
│                                         ▼       ▼           │
│                              ┌──────────────────┐           │
│                              │ Cloudflare Tunnel │           │
│                              └────────┬─────────┘           │
└───────────────────────────────────────┼─────────────────────┘
                                        │ Encrypted tunnel
                                        │ (no public IP needed)
                                        ▼
                              ┌──────────────────┐
                              │ AWS EC2 t4g.small │
                              │ (Mumbai)          │
                              │                   │
                              │ ┌──────────────┐  │
                              │ │ .NET 10 API  │  │
                              │ │ :5000        │  │
                              │ └──────┬───────┘  │
                              │        │          │
                              │ ┌──────▼───────┐  │
                              │ │ PostgreSQL16 │  │
                              │ │ :5432        │  │
                              │ └──────────────┘  │
                              └──────────────────┘
```

## Table of Contents

1. [AWS Setup](#1-aws-setup)
2. [Cloudflare Setup](#2-cloudflare-setup)
3. [Database Setup & Migrations](#3-database-setup--migrations)
4. [Deploy the .NET API](#4-deploy-the-net-api)
5. [WASM SPA Routing](#5-wasm-spa-routing)
6. [CORS Configuration](#6-cors-configuration)
7. [Environment Variables Reference](#7-environment-variables-reference)
8. [CI/CD Pipelines](#8-cicd-pipelines)
9. [Cost Breakdown](#9-cost-breakdown)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. AWS Setup

### 1.1 Create AWS Account

Sign up at [aws.amazon.com](https://aws.amazon.com). Accounts created after July 2025 get **$200 in free credits** — more than enough for this setup.

### 1.2 Launch EC2 Instance

Go to **EC2 → Launch Instance** in the `ap-south-1` (Mumbai) region.

| Setting | Value |
|---------|-------|
| **Name** | `realin-dev` |
| **AMI** | Ubuntu 24.04 LTS (ARM64) |
| **Instance type** | `t4g.small` (2 vCPU, 2 GB RAM) — free trial through Dec 2026 |
| **Key pair** | Create new → `realin-dev-key` (download `.pem` file) |
| **Storage** | 30 GB gp3 (default) |

#### Security Group

Since Cloudflare Tunnel handles all inbound traffic, we need minimal rules:

| Type | Port | Source | Purpose |
|------|------|--------|---------|
| SSH | 22 | My IP | SSH access for setup/deploys |

> No port 80/443 needed — Cloudflare Tunnel creates an outbound connection from EC2 to Cloudflare.

After launch, note the **Public IPv4** (only needed for initial SSH setup; you can remove the public IP later if desired, but keeping it is fine for SSH access).

### 1.3 SSH into EC2

```bash
chmod 400 realin-dev-key.pem
ssh -i realin-dev-key.pem ubuntu@<EC2_PUBLIC_IP>
```

### 1.4 Install .NET 10 Runtime (ARM64)

```bash
# Add Microsoft package repo
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh

# Install ASP.NET Core runtime (not full SDK — we build locally or in CI)
./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet
sudo ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet

# Verify
dotnet --info
```

> If you plan to build on EC2 (instead of publishing locally), install the full SDK:
> `./dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet`

### 1.5 Install PostgreSQL 16

```bash
sudo apt update
sudo apt install -y postgresql-16 postgresql-client-16

# Start and enable
sudo systemctl enable postgresql
sudo systemctl start postgresql
```

### 1.6 Install cloudflared

```bash
# ARM64 binary
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-arm64.deb -o cloudflared.deb
sudo dpkg -i cloudflared.deb

# Verify
cloudflared --version
```

---

## 2. Cloudflare Setup

> **Custom domain not required for dev.** Cloudflare gives you free subdomains (`*.pages.dev` for WASM, `*.cfargotunnel.com` for the API tunnel). You can add a custom domain later when going to production.

### 2.1 Add Domain to Cloudflare (skip if no custom domain)

If you own a domain (e.g., `realin.in`):

1. Go to [dash.cloudflare.com](https://dash.cloudflare.com) → **Add a site** → enter your domain
2. Select **Free plan**
3. Update your domain registrar's nameservers to the ones Cloudflare provides
4. Wait for DNS propagation (usually < 1 hour)

If you don't own a domain yet, skip this step. Everything below works with free Cloudflare subdomains.

### 2.2 R2 Bucket (Media Storage)

1. Go to **R2 Object Storage** → **Create bucket**
   - Bucket name: `realin-media`
   - Location: **Asia Pacific (APAC)** — closest to Mumbai
2. Go to **R2 → Overview → Manage R2 API Tokens** → **Create API Token**
   - Permissions: **Object Read & Write**
   - Scope: `realin-media` bucket only
   - Note down: **Access Key ID**, **Secret Access Key**, **Account ID**

The S3-compatible endpoint will be: `https://<ACCOUNT_ID>.r2.cloudflarestorage.com`

### 2.3 Cloudflare Tunnel

#### Authenticate cloudflared on EC2

```bash
cloudflared tunnel login
# Opens a URL — copy it to your browser, authorize your Cloudflare account
```

#### Create the tunnel

```bash
cloudflared tunnel create realin-dev
# Note the tunnel UUID output, e.g.: a1b2c3d4-...
```

#### Configure the tunnel

**Option A: Without a custom domain (free subdomain)**

The tunnel is automatically accessible at `https://<TUNNEL_UUID>.cfargotunnel.com`. Configure it to route to your local API:

```bash
mkdir -p ~/.cloudflared

cat > ~/.cloudflared/config.yml << 'EOF'
tunnel: <TUNNEL_UUID>
credentials-file: /home/ubuntu/.cloudflared/<TUNNEL_UUID>.json

ingress:
  - service: http://localhost:5000
EOF
```

Your API will be available at: `https://<TUNNEL_UUID>.cfargotunnel.com`

> Tip: Note down the full tunnel URL — you'll need it for the WASM app's `API_BASE_URL`.

**Option B: With a custom domain (e.g., `api.yourdomain.com`)**

```bash
mkdir -p ~/.cloudflared

cat > ~/.cloudflared/config.yml << 'EOF'
tunnel: <TUNNEL_UUID>
credentials-file: /home/ubuntu/.cloudflared/<TUNNEL_UUID>.json

ingress:
  - hostname: api.yourdomain.com
    service: http://localhost:5000
  - service: http_status:404
EOF
```

Then create the DNS record:

```bash
cloudflared tunnel route dns realin-dev api.yourdomain.com
# This auto-creates a CNAME record in Cloudflare DNS
```

Replace `<TUNNEL_UUID>` with the actual UUID from the create command.

#### Run tunnel as a systemd service

```bash
sudo cloudflared service install
sudo systemctl enable cloudflared
sudo systemctl start cloudflared

# Verify
sudo systemctl status cloudflared
cloudflared tunnel info realin-dev
```

### 2.4 Cloudflare Pages (Blazor WASM)

1. Go to **Workers & Pages** → **Create** → **Pages** → **Connect to Git**
2. Select the **RealinCloud** repository
3. Configure build:

| Setting | Value |
|---------|-------|
| **Production branch** | `main` |
| **Preview branches** | `develop` |
| **Build command** | `bash RealinAdmin/scripts/build-wasm.sh` |
| **Build output directory** | `RealinAdmin/src/RealEstate.Admin/bin/Release/net10.0/publish/wwwroot` |
| **Root directory** | `/` (repo root) |

4. Add environment variable:

| Variable | Production | Preview |
|----------|-----------|---------|
| `API_BASE_URL` | `https://<YOUR_API_URL>` | `https://<YOUR_API_URL>` |

Where `<YOUR_API_URL>` is either:
- **Without custom domain:** `<TUNNEL_UUID>.cfargotunnel.com`
- **With custom domain:** `api.yourdomain.com`

> The build script reads `API_BASE_URL` and writes it into the WASM app's `appsettings.json`.

5. Save and deploy. The WASM app will be available at:
   - Production: `realin-admin.pages.dev`
   - Preview: `develop.realin-admin.pages.dev`

#### Custom Domain for Pages (optional — requires owning a domain)

1. Go to your Pages project → **Custom domains** → **Set up a custom domain**
2. Enter `admin.yourdomain.com`
3. Cloudflare auto-creates the DNS record

### 2.5 What You Get — Summary

| | Without custom domain (free) | With custom domain |
|---|---|---|
| **WASM app** | `realin-admin.pages.dev` | `admin.yourdomain.com` |
| **API** | `<TUNNEL_UUID>.cfargotunnel.com` | `api.yourdomain.com` |
| **Preview** | `develop.realin-admin.pages.dev` | same |
| **Cost** | $0 | ~$3-10/year for domain |

---

## 3. Database Setup & Migrations

### 3.1 Configure PostgreSQL on EC2

```bash
# Switch to postgres user
sudo -u postgres psql

-- Create database and user
CREATE USER realin_app WITH PASSWORD 'YOUR_STRONG_PASSWORD';
CREATE DATABASE realin_dev OWNER realin_app;

-- Connect to the database
\c realin_dev

-- Create schema
CREATE SCHEMA realin AUTHORIZATION realin_app;

-- Set default schema for the user
ALTER USER realin_app SET search_path TO realin;

\q
```

### 3.2 Run Migrations

**Option A: SSH into EC2 (requires .NET SDK on EC2)**

```bash
# Clone repo on EC2
git clone https://github.com/<YOUR_ORG>/RealinCloud.git ~/RealinCloud
cd ~/RealinCloud/RealinAdmin

# Run migrations (pass connection string directly to avoid URL parsing issues)
dotnet ef database update \
  --project src/RealEstate.Infrastructure \
  --startup-project src/RealEstate.Api \
  --connection "Host=localhost;Port=5432;Database=realin_dev;Username=realin_app;Password=YOUR_STRONG_PASSWORD;SSL Mode=Disable;SearchPath=realin"
```

**Option B: From local machine via SSH tunnel**

```bash
# Terminal 1: SSH tunnel (use 5433 locally if your local PostgreSQL is on 5432)
ssh -i realin-dev-key.pem -L 5433:localhost:5432 ubuntu@<EC2_PUBLIC_IP>

# Terminal 2: Run migrations locally (pointed at tunneled port)
cd RealinAdmin
dotnet ef database update \
  --project src/RealEstate.Infrastructure \
  --startup-project src/RealEstate.Api \
  --connection "Host=localhost;Port=5433;Database=realin_dev;Username=realin_app;Password=YOUR_STRONG_PASSWORD;SSL Mode=Disable;SearchPath=realin"
```

> Using `--connection` passes the connection string directly to EF Core, bypassing any URL parsing. This avoids issues with special characters in passwords.

### 3.3 Seed SuperAdmin

```bash
# On EC2, after migrations
sudo -u postgres psql -d realin_dev -f ~/RealinCloud/RealinAdmin/scripts/seed-superadmin.sql
```

---

## 4. Deploy the .NET API

### 4.1 Create Environment File

```bash
sudo mkdir -p /etc/realin
sudo tee /etc/realin/realin-api.env > /dev/null << 'EOF'
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5000

# Database (local PostgreSQL, no SSL needed)
# Use .NET connection string format to avoid URL parsing issues with special characters in passwords
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=realin_dev;Username=realin_app;Password=YOUR_STRONG_PASSWORD;SSL Mode=Disable;SearchPath=realin

# JWT
Jwt__SecretKey=YOUR_JWT_SECRET_KEY_AT_LEAST_32_CHARACTERS
Jwt__Issuer=RealinApi
Jwt__Audience=RealinApp

# Storage (Cloudflare R2 — S3-compatible)
Storage__Provider=s3
Storage__S3__BucketName=realin-media
Storage__S3__Region=auto
Storage__S3__ServiceUrl=https://YOUR_ACCOUNT_ID.r2.cloudflarestorage.com
Storage__S3__AccessKey=YOUR_R2_ACCESS_KEY
Storage__S3__SecretKey=YOUR_R2_SECRET_KEY
Storage__S3__ForcePathStyle=true
EOF

# Secure the file
sudo chmod 600 /etc/realin/realin-api.env
sudo chown root:root /etc/realin/realin-api.env
```

### 4.2 Build and Publish the API

**Option A: Build on EC2 (requires .NET SDK)**

```bash
cd ~/RealinCloud/RealinAdmin
git pull origin develop

dotnet publish src/RealEstate.Api/RealEstate.Api.csproj \
  -c Release \
  -o /opt/realin-api
```

**Option B: Build locally, copy to EC2**

```bash
# On local machine
cd RealinAdmin
dotnet publish src/RealEstate.Api/RealEstate.Api.csproj \
  -c Release \
  -r linux-arm64 \
  --self-contained false \
  -o ./publish

# Copy to EC2
scp -i realin-dev-key.pem -r ./publish/* ubuntu@<EC2_PUBLIC_IP>:/opt/realin-api/
```

### 4.3 Create systemd Service

```bash
sudo tee /etc/systemd/system/realin-api.service > /dev/null << 'EOF'
[Unit]
Description=Realin API (.NET 10)
After=network.target postgresql.service
Requires=postgresql.service

[Service]
Type=exec
User=ubuntu
Group=ubuntu
WorkingDirectory=/opt/realin-api
ExecStart=/usr/local/bin/dotnet /opt/realin-api/RealEstate.Api.dll
EnvironmentFile=/etc/realin/realin-api.env
Restart=always
RestartSec=5
SyslogIdentifier=realin-api

# Hardening
NoNewPrivileges=true
ProtectSystem=strict
ReadWritePaths=/opt/realin-api/uploads

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable realin-api
sudo systemctl start realin-api

# Check status
sudo systemctl status realin-api
sudo journalctl -u realin-api -f
```

---

## 5. WASM SPA Routing

Cloudflare Pages needs a `_redirects` file to handle client-side routing. This is already included at:

```
src/RealEstate.Admin/wwwroot/_redirects
```

Contents:
```
/*    /index.html    200
```

This ensures all routes (e.g., `/properties`, `/agents/123`) serve `index.html` so Blazor's router handles them.

---

## 6. CORS Configuration

Currently the API allows any origin (`AllowAnyOrigin()`). For dev this works fine, but for production tighten it in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.WithOrigins(
                "https://realin-admin.pages.dev",        // Pages production
                "https://develop.realin-admin.pages.dev", // Pages preview
                "https://admin.yourdomain.com"            // Custom domain (if applicable)
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

> For now, `AllowAnyOrigin()` is acceptable in dev. Tighten when going to production.

---

## 7. Environment Variables Reference

### EC2 (systemd environment file: `/etc/realin/realin-api.env`)

| Variable | Example | Notes |
|----------|---------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | |
| `ASPNETCORE_URLS` | `http://localhost:5000` | Only listens on localhost; Cloudflare Tunnel handles external access |
| `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=realin_dev;Username=realin_app;Password=YOUR_PASSWORD;SSL Mode=Disable;SearchPath=realin` | .NET connection string format, avoids URL parsing issues |
| `Jwt__SecretKey` | (64-char random string) | Generate with `openssl rand -base64 48` |
| `Jwt__Issuer` | `RealinApi` | |
| `Jwt__Audience` | `RealinApp` | |
| `Storage__Provider` | `s3` | Enables S3StorageService |
| `Storage__S3__BucketName` | `realin-media` | R2 bucket name |
| `Storage__S3__Region` | `auto` | R2 uses `auto` |
| `Storage__S3__ServiceUrl` | `https://<ACCT_ID>.r2.cloudflarestorage.com` | R2 S3-compatible endpoint |
| `Storage__S3__AccessKey` | (from R2 API token) | |
| `Storage__S3__SecretKey` | (from R2 API token) | |
| `Storage__S3__ForcePathStyle` | `true` | Required for R2 |

### Cloudflare Pages (Build environment variables)

| Variable | Production | Preview |
|----------|-----------|---------|
| `API_BASE_URL` | `https://<YOUR_API_URL>` | `https://<YOUR_API_URL>` |

`<YOUR_API_URL>` is either `<TUNNEL_UUID>.cfargotunnel.com` (no domain) or `api.yourdomain.com` (custom domain).

---

## 8. CI/CD Pipelines

Two GitHub Actions workflows handle CI/CD. WASM deployment is handled separately by Cloudflare Pages (no Actions needed).

```
Push to develop
  ├── Cloudflare Pages — auto-builds WASM (native GitHub integration)
  ├── ci.yml — runs tests on any RealinAdmin change
  └── deploy.yml — builds + deploys API to EC2 (only if API code changed)

Pull Request → develop/main
  └── ci.yml — runs tests, blocks merge if failing
```

### 8.1 GitHub Secrets Setup

Go to your GitHub repo → **Settings** → **Environments** → create **development** → add secrets:

| Secret | Value | How to get it |
|--------|-------|---------------|
| `EC2_HOST` | EC2 public IPv4 address | AWS Console → EC2 → Instances → your instance |
| `EC2_SSH_KEY` | Contents of your `.pem` file | `cat realin-dev-key.pem` — copy the full output including `-----BEGIN` and `-----END` lines |

### 8.2 CI Pipeline (`.github/workflows/ci.yml`)

Runs on **all pushes and PRs** to `develop`/`main` when any `RealinAdmin/` file changes.

- Builds the full solution
- Runs all tests (Domain, Application, Infrastructure)
- Blocks PR merge if tests fail

### 8.3 API Deploy Pipeline (`.github/workflows/deploy.yml`)

Runs on **push to `develop`** only when API-related code changes:
- `RealinAdmin/src/RealEstate.Api/**`
- `RealinAdmin/src/RealEstate.Domain/**`
- `RealinAdmin/src/RealEstate.Application/**`
- `RealinAdmin/src/RealEstate.Infrastructure/**`

Steps: restore → build → test → publish (linux-arm64) → SCP to EC2 → restart systemd service → health check.

WASM-only changes (e.g., `src/RealEstate.Admin/**`) do **not** trigger this workflow — Cloudflare Pages handles WASM deploys automatically.

### 8.4 WASM Deploy (Cloudflare Pages — no Actions needed)

Cloudflare Pages watches the GitHub repo directly:

```
Push to develop → auto-build → Preview at develop.realin-admin.pages.dev
Push to main    → auto-build → Production at realin-admin.pages.dev
```

Build config is set in Cloudflare Dashboard (see section 2.4).

### 8.5 Database Migrations (Manual)

Migrations are **not** automated in CI/CD — schema changes should be reviewed before applying.

```bash
# SSH into EC2 and run manually
ssh -i realin-dev-key.pem ubuntu@<EC2_PUBLIC_IP>
cd ~/RealinCloud/RealinAdmin
git pull origin develop

dotnet ef database update \
  --project src/RealEstate.Infrastructure \
  --startup-project src/RealEstate.Api \
  --connection "Host=localhost;Port=5432;Database=realin_dev;Username=realin_app;Password=YOUR_STRONG_PASSWORD;SSL Mode=Disable;SearchPath=realin"
```

### 8.6 Manual API Deploy (fallback)

If GitHub Actions is down or you need to deploy without pushing:

```bash
ssh -i realin-dev-key.pem ubuntu@<EC2_PUBLIC_IP>

cd ~/RealinCloud/RealinAdmin
git pull origin develop

dotnet publish src/RealEstate.Api/RealEstate.Api.csproj \
  -c Release \
  -o /opt/realin-api

sudo systemctl restart realin-api
sudo journalctl -u realin-api --no-pager -n 20
```

---

## 9. Cost Breakdown

| Service | Monthly Cost | Notes |
|---------|-------------|-------|
| EC2 t4g.small | **$0** | Free trial through Dec 2026 |
| EBS 30 GB gp3 | **~$2.40** | Covered by $200 credits |
| Public IPv4 | **$0** | Not required (Cloudflare Tunnel handles ingress) |
| Cloudflare Pages | **$0** | Free tier: 500 builds/month |
| Cloudflare R2 | **$0** | Free tier: 10 GB storage, 10M reads/month |
| Cloudflare Tunnel | **$0** | Free |
| Cloudflare DNS | **$0** | Free |
| **Total** | **~$2.40/month** | Covered by AWS credits |

---

## 10. Troubleshooting

### API not responding via api.realin.com

```bash
# Check tunnel status
sudo systemctl status cloudflared
cloudflared tunnel info realin-dev

# Check API is running
sudo systemctl status realin-api
curl http://localhost:5000/openapi/v1.json

# Check tunnel config
cat ~/.cloudflared/config.yml

# Check tunnel logs
sudo journalctl -u cloudflared -f
```

### WASM app loads but API calls fail

1. **CORS error in browser console**: Check `AllowAnyOrigin()` is still set, or add the Pages domain to the CORS policy
2. **Network error**: Verify `appsettings.json` in WASM has the correct `ApiBaseUrl` (`https://api.realin.com`)
3. **Mixed content**: The API must be served over HTTPS (Cloudflare Tunnel does this automatically)

### Database connection issues

```bash
# Check PostgreSQL is running
sudo systemctl status postgresql

# Test connection
sudo -u postgres psql -d realin_dev -c "SELECT 1;"

# Check the API can connect
sudo journalctl -u realin-api --no-pager -n 50 | grep -i "database\|connection\|npgsql"
```

### Cloudflare Pages build fails

1. Check build logs in Cloudflare Dashboard → Pages → your project → Deployments
2. Verify `build-wasm.sh` installs the correct .NET SDK version
3. Ensure `API_BASE_URL` environment variable is set in Pages settings

### EC2 out of memory (2 GB RAM)

```bash
# Check memory usage
free -h

# Create swap file if needed
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

### Tunnel DNS not resolving

```bash
# Verify DNS record exists
dig api.realin.com

# Re-create DNS route
cloudflared tunnel route dns realin-dev api.realin.com
```

### Verify Everything Works

```bash
# 1. WASM app loads
curl -s https://develop.realin-admin.pages.dev | head -5

# 2. API responds through tunnel (use your actual API URL)
#    Without custom domain:
curl -s https://<TUNNEL_UUID>.cfargotunnel.com/openapi/v1.json | head -5
#    With custom domain:
curl -s https://api.yourdomain.com/openapi/v1.json | head -5

# 3. Database accessible on EC2
sudo -u postgres psql -d realin_dev -c "SELECT current_schema();"

# 4. Tunnel is healthy
cloudflared tunnel info realin-dev

# 5. R2 credentials work (from EC2)
curl -s --head \
  --aws-sigv4 "aws:amz:auto:s3" \
  --user "$R2_ACCESS_KEY:$R2_SECRET_KEY" \
  "https://YOUR_ACCOUNT_ID.r2.cloudflarestorage.com/realin-media"
```
