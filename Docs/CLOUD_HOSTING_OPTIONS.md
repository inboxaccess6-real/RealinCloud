# RealinAdmin — Cloud Hosting Options (Free Tier)

**Date:** February 19, 2026
**Goal:** Host the .NET API + Blazor WASM app + PostgreSQL for testing/development at zero or near-zero cost.

---

## What Needs Hosting

| Component | Technology | Requirements |
|-----------|-----------|--------------|
| **Frontend** | Blazor WASM (static files) | Static file hosting, HTTPS, custom domain optional |
| **Backend API** | ASP.NET (.NET 10) | Runs as a web server, needs persistent storage for uploads |
| **Database** | PostgreSQL | Relational DB, ~500MB for testing |
| **File Storage** | Local uploads (images/videos) | Currently local filesystem, can switch to S3/blob |

---

## Option 1: Azure (Recommended for .NET)

Best native .NET support since it's Microsoft's cloud.

### Frontend — Azure Static Web Apps (Free)
- **Free tier:** 100GB bandwidth/month, 2 custom domains, free SSL
- Perfect for Blazor WASM — just deploy the `wwwroot` output
- Built-in CI/CD with GitHub Actions
- Global CDN included

### Backend API — Azure App Service (Free Tier)
- **Free tier (F1):** 1GB RAM, 1GB storage, 60 min CPU/day, shared compute
- Supports .NET 10 natively
- Limitation: sleeps after inactivity (cold starts ~10-15s), 60 min/day CPU limit
- Good enough for light testing

### Alternative API — Azure Container Apps (Consumption)
- **Free grant:** 180,000 vCPU-seconds + 360,000 GiB-seconds/month
- Can scale to zero (no cost when idle)
- Deploy as Docker container
- Better than App Service Free for intermittent testing

### Database — Azure Database for PostgreSQL (Flexible Server)
- **Free tier:** Burstable B1ms (1 vCPU, 2GB RAM, 32GB storage) — free for 12 months
- Full managed PostgreSQL
- Best option if you're already in Azure

### File Storage — Azure Blob Storage
- **Free tier:** 5GB LRS storage, 20,000 read + 10,000 write operations/month (12 months)
- Replace LocalFileStorageService with Azure Blob SDK

### Azure Total: $0/month (within free tier limits)

---

## Option 2: AWS

### Frontend — S3 + CloudFront
- **S3 free tier:** 5GB storage, 20,000 GET requests/month (12 months)
- **CloudFront:** 1TB data transfer, 10M requests/month (always free)
- Host Blazor WASM as static website from S3 bucket
- CloudFront adds HTTPS + CDN

### Backend API — AWS Lambda + API Gateway
- **Lambda free:** 1M requests + 400,000 GB-seconds/month (always free)
- **API Gateway:** 1M REST API calls/month (12 months)
- Need to package API as Lambda function (uses `Amazon.Lambda.AspNetCoreServer`)
- Cold starts can be 3-5s for .NET
- No persistent local filesystem (uploads must go to S3)

### Alternative API — AWS EC2 (t2.micro / t3.micro)
- **Free tier:** 750 hours/month of t2.micro (1 vCPU, 1GB RAM) — 12 months
- Always-on, full control
- Can store uploads locally on the instance
- Need to manage OS updates, security yourself

### Database — AWS RDS PostgreSQL
- **Free tier:** db.t3.micro (1 vCPU, 1GB RAM, 20GB storage) — 12 months
- Managed PostgreSQL, automated backups

### File Storage — S3
- **Free tier:** 5GB storage (12 months)
- Already have `S3StorageService` implemented

### AWS Total: $0/month (within free tier limits, 12-month expiry on most)

---

## Option 3: Google Cloud (GCP)

### Frontend — Firebase Hosting or Cloud Storage
- **Firebase Hosting free:** 10GB storage, 360MB/day transfer
- Easy deploy for static sites
- Free SSL, global CDN

### Backend API — Cloud Run
- **Free tier:** 2M requests/month, 360,000 vCPU-seconds, 180,000 GiB-seconds (always free)
- Deploy as Docker container
- Scales to zero (no cost when idle)
- Good .NET support via containers
- No persistent local filesystem (uploads need Cloud Storage)

### Database — No free managed PostgreSQL
- GCP doesn't offer free-tier Cloud SQL for PostgreSQL
- Use **Neon** or **Supabase** instead (see below)

### Alternative — Compute Engine (e1-micro)
- **Free tier:** 1 e2-micro VM (0.25 vCPU, 1GB RAM) — always free (US regions only)
- Can run API + PostgreSQL on same VM
- 30GB standard persistent disk included
- Limitation: US regions only (us-west1, us-central1, us-east1)

### GCP Total: $0/month (e2-micro is always-free, others have 12-month limits)

---

## Option 4: Platform-as-a-Service (PaaS)

Simpler deployment, less infrastructure to manage.

### Render
- **Free tier:** Static sites free, 1 web service (512MB RAM), spins down after 15 min inactivity
- Supports Docker → can run .NET API
- Free PostgreSQL: 256MB storage, expires after 90 days
- Cold starts after inactivity (~30s for .NET)
- Simple GitHub integration

### Fly.io
- **Free tier:** 3 shared VMs (256MB RAM each), 3GB persistent storage, 100GB bandwidth
- Deploy .NET as Docker container
- Always-on (no sleep), static IPs included
- Free managed PostgreSQL (1GB storage)
- Deploy globally (20+ regions)

### Railway
- **No free tier** — minimum $5/month + usage
- Not recommended for zero-cost testing

---

## Option 5: Free PostgreSQL-Only Hosting

If you pick a cloud that doesn't have free PostgreSQL (or you want a separate managed DB):

| Provider | Free Tier | Storage | Notes |
|----------|----------|---------|-------|
| **Neon** | Always free | 0.5GB storage, 191 compute-hours/month | Serverless, scales to zero, branching for dev/test |
| **Supabase** | Always free | 500MB database, 1GB file storage | Includes auth, storage, realtime — full backend |
| **Azure Flexible** | 12 months free | 32GB | Best specs, time-limited |
| **AWS RDS** | 12 months free | 20GB | Managed, time-limited |
| **Fly.io** | Always free | 1GB | Managed Postgres, simple setup |

---

## Recommendation for Your Situation

You need: .NET API + Blazor WASM + PostgreSQL + file uploads, zero cost, for testing.

### Best Option: Azure (all-in-one)

| Component | Service | Cost |
|-----------|---------|------|
| Frontend | Azure Static Web Apps (Free) | $0 |
| API | Azure App Service F1 or Container Apps | $0 |
| Database | Azure PostgreSQL Flexible (Free 12 months) | $0 |
| File Storage | Azure Blob Storage (Free 12 months) | $0 |

**Why:** Best .NET support, everything in one place, 12 months free on most services, and Static Web Apps is always-free. Microsoft ecosystem = best tooling for .NET.

### Runner-Up: Fly.io (All-in-One)

| Component | Service | Cost |
|-----------|---------|------|
| Frontend | Fly.io static site or same app | $0 |
| API | Fly.io (Docker container, always-on) | $0 |
| Database | Fly.io Postgres (1GB free) | $0 |
| File Storage | Fly.io persistent volume (3GB free) | $0 |

**Why:** Everything in one place, always-free (no 12-month expiry), no cold starts, simple Docker deploy. Free Postgres included — no external DB needed.

### Simplest Option: GCP e2-micro VM

| Component | Service | Cost |
|-----------|---------|------|
| Everything | 1 GCP e2-micro VM (US region) | $0 |

Run API + PostgreSQL + serve WASM files all from one always-free VM. Most like your current local setup. Simplest migration path — just `scp` your code and run it.

---

## File / Media Storage — Cloud Options

Your app currently uses `LocalFileStorageService` (saves to disk) with an `S3StorageService` already implemented. Since this is a real estate platform, media (property images, videos) will grow significantly. Choosing the right storage matters for both cost and performance at scale.

### Comparison Table

| Provider | Storage Cost | Egress (Bandwidth) | Free Tier | S3 Compatible | Best For |
|----------|-------------|---------------------|-----------|---------------|----------|
| **Cloudflare R2** | $0.015/GB/mo | **$0 (free forever)** | 10GB storage, 1M writes, 10M reads/mo | Yes | Production at scale — zero egress is a game-changer for media-heavy apps |
| **AWS S3** | $0.023/GB/mo | $0.09/GB after 100GB free/mo | 5GB storage (12 months) | Yes (native) | Already implemented in your codebase (`S3StorageService`) |
| **Azure Blob** | $0.018/GB/mo | $0.087/GB after 100GB free/mo | 5GB LRS (12 months) | Via SDK (not S3 API) | Best if hosting API on Azure |
| **Backblaze B2** | $0.006/GB/mo | Free up to 3x stored data/mo | 10GB storage | Yes | Cheapest raw storage, good CDN partnerships |
| **DigitalOcean Spaces** | $5/mo flat | 1TB included, then $0.01/GB | None (starts at $5) | Yes | Simple flat pricing, no surprises |
| **GCP Cloud Storage** | $0.020/GB/mo | $0.12/GB | 5GB (always free) | Via SDK | Best if hosting API on GCP |
| **Supabase Storage** | Free tier: 1GB | 2GB bandwidth on free | 1GB storage | No (custom API) | Already using Supabase for DB? Bundle it |
| **Wasabi** | $0.0069/GB/mo | **$0 (free)** | None (pay-as-you-go) | Yes | Bulk archival, 90-day minimum retention |

### Cost Projection at Scale

For a real estate platform with **10,000 properties, ~5 images each (avg 2MB), 50GB total storage, 500GB egress/month:**

| Provider | Storage/mo | Egress/mo | Total/mo |
|----------|-----------|-----------|----------|
| **Cloudflare R2** | $0.75 | $0 | **$0.75** |
| **Backblaze B2** | $0.30 | $0 (within 3x limit) | **$0.30** |
| **AWS S3** | $1.15 | $36.00 | **$37.15** |
| **Azure Blob** | $0.92 | $34.80 | **$35.72** |
| **DigitalOcean Spaces** | $5.00 | $0 (within 1TB) | **$5.00** |
| **GCP Cloud Storage** | $1.00 | $48.00 | **$49.00** |

### Recommendation

**For testing now:** Keep `LocalFileStorageService` (free, simple). If deploying to a serverless/containerized platform without persistent disk, use **Cloudflare R2** free tier (10GB).

**For production scaling:** **Cloudflare R2** is the clear winner for media-heavy apps like real estate:
- Zero egress fees means serving thousands of property images costs only the storage ($0.015/GB)
- S3-compatible API — your existing `S3StorageService` works with minimal config changes (just change endpoint URL and credentials)
- No 12-month free tier expiry
- Global edge caching via Cloudflare's CDN network
- 10GB free tier covers early production

**Runner-up:** **Backblaze B2** if you want the absolute cheapest storage at $0.006/GB and can pair it with Cloudflare CDN for free egress (Backblaze has a bandwidth alliance with Cloudflare).

### Integration Notes

Your codebase already has `IFileStorageService` with two implementations:
- `LocalFileStorageService` — saves to disk (current)
- `S3StorageService` — full AWS S3 SDK implementation

For Cloudflare R2, Backblaze B2, or DigitalOcean Spaces, the existing `S3StorageService` works as-is since they all expose an S3-compatible API. You only need to change the endpoint URL, access key, and bucket name in config:

```json
// appsettings.json for Cloudflare R2
{
  "Storage": {
    "Provider": "s3",
    "S3": {
      "ServiceUrl": "https://<account-id>.r2.cloudflarestorage.com",
      "AccessKey": "your-r2-access-key",
      "SecretKey": "your-r2-secret-key",
      "BucketName": "realin-media",
      "ForcePathStyle": true
    }
  }
}
```

For Azure Blob Storage, you'd need a new `AzureBlobStorageService` implementing `IFileStorageService` (different SDK, not S3 compatible).

---

## Deployment Checklist (When Ready)

- [ ] Dockerize the API (`Dockerfile`)
- [ ] Set up production `appsettings.Production.json` (connection string, CORS origins, JWT secrets)
- [ ] Switch file storage to cloud blob/S3 (or keep local if using VM)
- [ ] Build Blazor WASM for production (`dotnet publish -c Release`)
- [ ] Set up CI/CD (GitHub Actions recommended)
- [ ] Configure HTTPS / SSL certificate
- [ ] Set environment-specific API base URL in WASM `appsettings.json`

---

## Sources

- [Azure Hosting Options for Blazor 2026](https://www.gapvelocity.ai/blog/azure-hosting-options-for-blazor-in-2026-app-service-vs-static-web-apps-vs-container-apps-vs-aks)
- [Top PostgreSQL Free Tiers 2026 — Koyeb](https://www.koyeb.com/blog/top-postgresql-database-free-tiers-in-2026)
- [Neon Pricing](https://neon.com/pricing)
- [Supabase vs Neon Comparison](https://www.freetiers.com/blog/supabase-vs-neon-comparison)
- [Railway vs Render 2026 — Northflank](https://northflank.com/blog/railway-vs-render)
- [Fly.io vs Render 2026 — Northflank](https://northflank.com/blog/flyio-vs-render)
- [AWS vs Azure vs GCP Free Tier Comparison](https://mindmajix.com/aws-vs-azure-vs-google-cloud-free-tier)
- [Deploy Blazor WASM to Azure — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/?view=aspnetcore-10.0)
- [Cloudflare R2 Pricing](https://developers.cloudflare.com/r2/pricing/)
- [Cloudflare R2 vs Big 3 — Cost & Technical Comparison](https://yconsulting.substack.com/p/cloudflare-r2-vs-the-big-3-a-deep)
- [Cloud Storage Pricing Comparison — Backblaze](https://www.backblaze.com/cloud-storage/pricing)
- [S3 Compatible Object Storage Benchmark 2026](https://research.aimultiple.com/s3-compatible-object-storage/)
- [Cloudflare R2 vs Backblaze B2 vs Wasabi vs S3](https://onidel.com/blog/cloudflare-r2-vs-backblaze-b2)
- [Amazon S3 Alternatives — DigitalOcean](https://www.digitalocean.com/resources/articles/amazon-s3-alternatives)
