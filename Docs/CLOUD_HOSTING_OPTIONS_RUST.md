# RealinAdmin (Rust Stack) — Cloud Hosting Options (Free Tier)

**Date:** February 19, 2026
**Goal:** Host a Rust backend (Axum/Actix) + Leptos WASM frontend + PostgreSQL for testing/development at zero or near-zero cost, with a path to production-grade scaling.

---

## What Needs Hosting

| Component | Technology | Requirements |
|-----------|-----------|--------------|
| **Frontend** | Leptos CSR (compiles to WASM + static files) | Static file hosting, HTTPS, custom domain optional |
| **Desktop App** | Tauri + Leptos | Not hosted — distributed as native binary (Windows/Mac/Linux) |
| **Backend API** | Rust (Axum or Actix Web) | Single compiled binary, needs persistent storage for uploads |
| **Database** | PostgreSQL (via SQLx or Diesel) | Relational DB, ~500MB for testing |
| **File Storage** | Local uploads or S3-compatible | Property images and videos |

### Tauri vs Leptos Web — Hosting Implications

- **Tauri** is a desktop/mobile framework — the app ships as a native binary. No cloud hosting needed for the app itself. The API still needs hosting.
- **Leptos CSR** (client-side rendered) compiles to WASM + static HTML/CSS/JS — hosted exactly like Blazor WASM (static file hosting).
- **Leptos SSR** (server-side rendered) runs as a Rust binary on the server — needs a container/VM (not static hosting).
- **Recommended for your case:** Leptos CSR for web + Tauri for desktop, sharing the same Rust API backend.

---

## Rust Advantage: Performance & Cost

Rust's compiled binaries give significant hosting advantages over .NET/Node.js:

| Metric | Rust (Axum) | .NET 10 | Node.js |
|--------|-------------|---------|---------|
| **Cold start (AWS Lambda)** | ~16ms (arm64) | ~250-400ms | ~100-200ms |
| **Memory usage (idle API)** | 12-20 MB | 80-150 MB | 50-80 MB |
| **Docker image size** | 10-50 MB (Alpine multi-stage) | 200-400 MB | 150-300 MB |
| **Binary size** | 5-15 MB (release, stripped) | N/A (needs runtime) | N/A (needs runtime) |
| **Startup time (container)** | <100ms | 1-3s | 500ms-1s |
| **Requests/sec (simple JSON)** | ~300K | ~150K | ~50K |

**What this means for hosting costs:**
- Rust fits comfortably in the smallest free-tier instances (256MB RAM)
- Near-zero cold starts make serverless (Lambda, Cloud Run) excellent — no user-facing delay
- Tiny Docker images = faster deploys, less storage
- Low memory = more headroom before hitting limits

---

## Option 1: Azure

### Frontend (Leptos CSR) — Azure Static Web Apps (Free)
- **Free tier:** 100GB bandwidth/month, 2 custom domains, free SSL
- Deploy the `dist/` output from `trunk build --release`
- Built-in CI/CD with GitHub Actions
- Global CDN included
- **Startup time:** N/A (static files, instant)

### Backend API — Azure Container Apps (Consumption)
- **Free grant:** 180,000 vCPU-seconds + 360,000 GiB-seconds/month
- Scale to zero when idle (no cost)
- Deploy Rust binary as Docker container (~15MB image)
- **Startup time:** <100ms (Rust binary, near-instant)
- Perfect for Rust — tiny image, low memory, fast start

### Alternative API — Azure App Service (Free Tier F1)
- **Free tier:** 1GB RAM, 1GB storage, 60 min CPU/day, shared compute
- Deploy as Docker container
- Limitation: sleeps after inactivity, 60 min/day CPU limit
- **Startup time:** <1s (Rust), compared to 10-15s for .NET on free tier

### Database — Azure Database for PostgreSQL (Flexible Server)
- **Free tier:** Burstable B1ms (1 vCPU, 2GB RAM, 32GB storage) — free for 12 months
- Full managed PostgreSQL

### File Storage — Azure Blob Storage
- **Free tier:** 5GB LRS storage, 20,000 read + 10,000 write ops/month (12 months)
- Use `aws-sdk-s3` Rust crate with Azure's S3-compatible endpoint, or `azure_storage_blobs` crate

### Azure Total: $0/month (within free tier limits)

---

## Option 2: AWS

### Frontend (Leptos CSR) — S3 + CloudFront
- **S3 free tier:** 5GB storage, 20,000 GET requests/month (12 months)
- **CloudFront:** 1TB data transfer, 10M requests/month (always free)
- Upload `trunk build --release` output to S3 bucket
- **Startup time:** N/A (static files served from CDN)

### Backend API — AWS Lambda (Rust)
- **Free tier:** 1M requests + 400,000 GB-seconds/month (always free)
- Rust is the **best language for Lambda** — 16ms cold starts on arm64
- Use `lambda_http` crate with Axum
- Tiny memory footprint = more free-tier headroom
- No persistent local filesystem (uploads must go to S3)
- **Startup time:** ~16ms cold start (arm64), effectively zero warm

### Alternative API — AWS EC2 (t2.micro / t3.micro)
- **Free tier:** 750 hours/month of t2.micro (1 vCPU, 1GB RAM) — 12 months
- Always-on, full control, persistent disk
- Rust API uses ~15MB RAM — enormous headroom on 1GB instance
- **Startup time:** <100ms (binary execution)

### Database — AWS RDS PostgreSQL
- **Free tier:** db.t3.micro (1 vCPU, 1GB RAM, 20GB storage) — 12 months

### File Storage — S3
- **Free tier:** 5GB storage (12 months)
- Use `aws-sdk-s3` Rust crate (native, well-supported)

### AWS Total: $0/month (Lambda always-free is the standout)

---

## Option 3: Google Cloud (GCP)

### Frontend (Leptos CSR) — Firebase Hosting or Cloud Storage
- **Firebase Hosting free:** 10GB storage, 360MB/day transfer
- Easy deploy for static sites
- **Startup time:** N/A (static files)

### Backend API — Cloud Run
- **Free tier:** 2M requests/month, 360,000 vCPU-seconds, 180,000 GiB-seconds (always free)
- Deploy Rust binary as Docker container
- Scales to zero (no cost when idle)
- **Startup time:** <200ms (container pull + Rust binary start)
- Rust's tiny image = faster cold starts than any other language on Cloud Run

### Database — No free managed PostgreSQL
- Use **Neon** or **Supabase** instead (see standalone DB options below)

### Alternative — Compute Engine (e2-micro)
- **Free tier:** 1 e2-micro VM (0.25 vCPU, 1GB RAM) — always free (US regions only)
- Run API + PostgreSQL on same VM
- Rust's 15MB memory footprint leaves plenty of room for PostgreSQL
- **Startup time:** <100ms (binary execution)

### GCP Total: $0/month (e2-micro always-free, Cloud Run always-free)

---

## Option 4: Rust-Native PaaS

### Shuttle.rs (Built for Rust)
- **Free tier (Community):** 1 project, shared resources, 0.5GB DB storage, 1GB egress
- Native Rust hosting — just `cargo shuttle deploy`
- Auto-provisions PostgreSQL, no config files needed
- Supports Axum, Actix Web, Rocket out of the box
- **Startup time:** <100ms (native Rust binary)
- **Limitation:** Free tier is limited (0.5GB DB, 1GB egress); Pro starts at $250/month (not great for scaling)
- **Best for:** Quick prototyping, learning Rust backends

### Fly.io
- **Free tier:** 3 shared VMs (256MB RAM each), 3GB persistent storage, 100GB bandwidth
- Deploy Rust binary as Docker container (~15MB image)
- Always-on (no sleep), static IPs included
- Free managed PostgreSQL (1GB storage)
- Deploy globally (20+ regions)
- **Startup time:** <100ms (always-on, no cold start)
- **Scaling:** Pay-as-you-go beyond free tier, add VMs as needed
- **Best for:** Always-on APIs with global distribution

### Render
- **Free tier:** 1 web service (512MB RAM), static sites free, spins down after 15 min inactivity
- Supports Docker → deploy Rust binary
- Free PostgreSQL: 256MB storage, expires after 90 days
- **Startup time:** ~1-2s after sleep (container restart), <100ms once running
- **Limitation:** 90-day DB expiry, sleep on inactivity

### Railway
- **No free tier** — minimum $5/month + usage
- Not recommended for zero-cost testing

---

## Option 5: Free PostgreSQL-Only Hosting

If your cloud doesn't include free PostgreSQL:

| Provider | Free Tier | Storage | Startup | Notes |
|----------|----------|---------|---------|-------|
| **Neon** | Always free | 0.5GB, 191 compute-hours/mo | ~200ms (scale from zero) | Serverless, branching, Rust `sqlx` works great |
| **Supabase** | Always free | 500MB database, 1GB file storage | Always on | Includes auth, storage, realtime |
| **Azure Flexible** | 12 months free | 32GB | Always on | Best specs, time-limited |
| **AWS RDS** | 12 months free | 20GB | Always on | Managed, time-limited |
| **Fly.io Postgres** | Always free | 1GB | Always on | Simple setup, co-locate with API |

---

## File / Media Storage — Cloud Options

Same options as the .NET stack. Your Rust backend would use the `aws-sdk-s3` crate for all S3-compatible providers.

| Provider | Storage Cost | Egress | Free Tier | S3 Compatible |
|----------|-------------|--------|-----------|---------------|
| **Cloudflare R2** | $0.015/GB/mo | **$0 (free)** | 10GB storage/mo | Yes |
| **AWS S3** | $0.023/GB/mo | $0.09/GB after 100GB free | 5GB (12 months) | Yes (native) |
| **Azure Blob** | $0.018/GB/mo | $0.087/GB after 100GB free | 5GB (12 months) | Via SDK |
| **Backblaze B2** | $0.006/GB/mo | Free up to 3x stored/mo | 10GB storage | Yes |
| **DigitalOcean Spaces** | $5/mo flat | 1TB included | None | Yes |

### Cost Projection at Scale (10K properties, 50GB storage, 500GB egress/month)

| Provider | Storage/mo | Egress/mo | Total/mo |
|----------|-----------|-----------|----------|
| **Cloudflare R2** | $0.75 | $0 | **$0.75** |
| **Backblaze B2** | $0.30 | $0 | **$0.30** |
| **AWS S3** | $1.15 | $36.00 | **$37.15** |

**Recommendation:** Cloudflare R2 — zero egress, S3-compatible, 10GB free. Use `aws-sdk-s3` crate with R2 endpoint.

---

## Production Scaling Comparison

When you're ready to move from free tier to production:

| Aspect | Rust | .NET |
|--------|------|------|
| **Instances needed for same load** | 1x | 3-5x (higher memory/CPU per request) |
| **Cost at scale (compute)** | 60-80% less than .NET | Baseline |
| **Serverless fit** | Excellent (16ms cold start) | Poor-to-fair (250ms+ cold start) |
| **Container density** | 10-20 containers per VM | 2-5 containers per VM |
| **Memory per instance** | 64-128 MB typical | 256-512 MB typical |
| **Horizontal scaling** | Start at 1, scale late | Start at 2-3, scale early |

### Production Architecture (Recommended)

```
                    ┌─────────────────────┐
                    │  Cloudflare CDN     │
                    │  (R2 for media)     │
                    └──────┬──────────────┘
                           │
              ┌────────────┴────────────┐
              │                         │
    ┌─────────▼─────────┐    ┌─────────▼─────────┐
    │  Leptos CSR/WASM  │    │  Tauri Desktop    │
    │  (Static hosting) │    │  (Native binary)  │
    └─────────┬─────────┘    └─────────┬─────────┘
              │                         │
              └────────────┬────────────┘
                           │
                  ┌────────▼────────┐
                  │  Rust API       │
                  │  (Axum/Actix)   │
                  │  Cloud Run /    │
                  │  Fly.io / ECS   │
                  └────────┬────────┘
                           │
              ┌────────────┴────────────┐
              │                         │
    ┌─────────▼─────────┐    ┌─────────▼─────────┐
    │  PostgreSQL       │    │  Cloudflare R2    │
    │  (Neon / RDS)     │    │  (Media storage)  │
    └───────────────────┘    └───────────────────┘
```

---

## Recommendation for Your Situation

You need: Rust API + Leptos WASM + PostgreSQL + file uploads, zero cost, for testing.

### Best Option: AWS Lambda + S3 + CloudFront

| Component | Service | Cost | Startup |
|-----------|---------|------|---------|
| Frontend | S3 + CloudFront (static WASM) | $0 | Instant (CDN) |
| API | AWS Lambda (Rust, arm64) | $0 | ~16ms cold start |
| Database | AWS RDS PostgreSQL (free 12 mo) | $0 | Always on |
| File Storage | S3 (free 12 months) | $0 | N/A |

**Why:** Rust + Lambda is a perfect match — 16ms cold starts mean serverless feels like always-on. 1M free requests/month is generous. The `lambda_http` crate makes Axum work on Lambda with minimal code changes. Scales to millions of requests without re-architecting.

### Runner-Up: Fly.io (Always-On, All-in-One)

| Component | Service | Cost | Startup |
|-----------|---------|------|---------|
| Frontend | Fly.io (same container or static) | $0 | Instant |
| API | Fly.io VM (256MB, Docker) | $0 | <100ms (always on) |
| Database | Fly.io Postgres (1GB free) | $0 | Always on |
| File Storage | Fly.io persistent volume (3GB free) | $0 | N/A |

**Why:** Everything in one place, always-free (no 12-month expiry), no cold starts, global deployment. Rust's 15MB memory footprint fits perfectly in the 256MB free VM. Free Postgres included — no external DB needed. Simple Docker deploy.

### Simplest Option: GCP e2-micro VM

| Component | Service | Cost | Startup |
|-----------|---------|------|---------|
| Everything | 1 GCP e2-micro VM (US region) | $0 | <100ms |

Run API + PostgreSQL + serve WASM files all from one always-free VM. Rust's tiny footprint (15MB API + ~100MB PostgreSQL) leaves plenty of room on a 1GB VM. Most like your current local setup.

### Rust-Native: Shuttle.rs (Quickest Deploy)

| Component | Service | Cost | Startup |
|-----------|---------|------|---------|
| API + DB | Shuttle.rs Community | $0 | <100ms |
| Frontend | Shuttle static or external CDN | $0 | Instant |
| File Storage | Cloudflare R2 (10GB free) | $0 | N/A |

**Why:** `cargo shuttle deploy` — one command. Auto-provisions PostgreSQL. But limited free tier (0.5GB DB, 1GB egress) and expensive Pro tier ($250/month) makes it poor for scaling.

---

## Rust-Specific Deployment Notes

### Building for Production
```bash
# Leptos CSR frontend (WASM output)
trunk build --release
# Output in dist/ → deploy to static hosting

# Rust API (optimized binary)
cargo build --release --target x86_64-unknown-linux-musl
# Binary in target/release/ → ~10-15MB, statically linked

# Docker (multi-stage, minimal image)
# Final image: Alpine + binary → ~15-50MB total
```

### Dockerfile Template (Axum API)
```dockerfile
FROM rust:1.85 AS builder
WORKDIR /app
COPY . .
RUN cargo build --release

FROM gcr.io/distroless/cc-debian12
COPY --from=builder /app/target/release/realin-api /
EXPOSE 8080
CMD ["/realin-api"]
```
Final image size: ~30-50MB (vs ~400MB for .NET)

### Key Rust Crates for Cloud
| Crate | Purpose |
|-------|---------|
| `axum` | Web framework (recommended) |
| `sqlx` | Async PostgreSQL with compile-time checked queries |
| `aws-sdk-s3` | S3/R2/B2 file storage |
| `lambda_http` | Run Axum on AWS Lambda |
| `tower-http` | CORS, compression, tracing middleware |
| `leptos` | Frontend framework (CSR/SSR) |
| `trunk` | WASM build tool for Leptos CSR |

---

## Deployment Checklist (When Ready)

- [ ] Build Leptos CSR frontend (`trunk build --release`)
- [ ] Build Rust API binary (`cargo build --release`)
- [ ] Create multi-stage Dockerfile for API
- [ ] Set up production config (connection string, CORS origins, JWT secrets)
- [ ] Switch file storage to Cloudflare R2 or S3
- [ ] Set up CI/CD (GitHub Actions recommended)
- [ ] Configure HTTPS / SSL certificate
- [ ] Set environment-specific API base URL in Leptos frontend config
- [ ] If using Lambda: add `lambda_http` crate and Lambda handler wrapper
- [ ] If using Tauri: build desktop binaries for target platforms

---

## Sources

- [Rust Web Frameworks 2026: Axum vs Actix](https://aarambhdevhub.medium.com/rust-web-frameworks-in-2026-axum-vs-actix-web-vs-rocket-vs-warp-vs-salvo-which-one-should-you-2db3792c79a2)
- [Lambda Cold Starts Benchmark](https://maxday.github.io/lambda-perf/)
- [Arm64 Crushes x86 in 2025 Lambda Benchmarks](https://www.techradar.com/pro/arm64-dominates-aws-lambda-in-2025-rust-4-5x-faster-than-x86-costs-30-less-across-all-workloads)
- [Why You Should Consider Rust for Lambdas](https://loige.co/why-you-should-consider-rust-for-your-lambdas/)
- [Leptos Deployment Guide — SSR](https://book.leptos.dev/deployment/ssr.html)
- [Leptos WASM Binary Size Optimization](https://book.leptos.dev/deployment/binary_size.html)
- [Tauri + Leptos Frontend](https://v2.tauri.app/start/frontend/leptos/)
- [Shuttle.rs Pricing](https://www.shuttle.dev/pricing)
- [Shuttle Pricing Update 2025](https://www.shuttle.dev/blog/2025/03/19/pricing-update)
- [Building Production Web Services with Rust and Axum](https://dasroot.net/posts/2026/01/building-production-web-services-rust-axum/)
- [Creating Lightweight Docker Images with Rust](https://medium.com/@pabloperezaradros/creating-lightweight-docker-images-with-rust-0db47cb014a9)
- [Cloudflare R2 Pricing](https://developers.cloudflare.com/r2/pricing/)
- [Cloud Storage Pricing Comparison — Backblaze](https://www.backblaze.com/cloud-storage/pricing)
- [Top PostgreSQL Free Tiers 2026 — Koyeb](https://www.koyeb.com/blog/top-postgresql-database-free-tiers-in-2026)
- [Neon Pricing](https://neon.com/pricing)
- [AWS vs Azure vs GCP Free Tier Comparison](https://mindmajix.com/aws-vs-azure-vs-google-cloud-free-tier)
- [Fly.io vs Render 2026 — Northflank](https://northflank.com/blog/flyio-vs-render)
