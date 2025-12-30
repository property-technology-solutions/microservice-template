# 🏢 Enterprise Deployment Guide

This guide explains how to deploy and use the microservice template in an enterprise environment.

---

## 📍 Repository Structure

```
GitHub Organization: property-technology-solutions
├── microservice-template/          ← This repo (template + BuildingBlocks)
├── order-service/                  ← Independent service repo
├── payment-service/                ← Independent service repo
└── inventory-service/              ← Independent service repo
```

**Key Point:** Each microservice lives in its own repository and only depends on BuildingBlocks via NuGet packages.

---

## 🚀 Developer Onboarding

### Prerequisites

- [ ] .NET 9.0 SDK installed
- [ ] Docker Desktop installed
- [ ] Git installed
- [ ] IDE (Rider / VS2022 / VS Code)
- [ ] GitHub access to `property-technology-solutions` organization

### First-Time Setup

```bash
# 1. Clone template repository
git clone https://github.com/property-technology-solutions/microservice-template.git
cd microservice-template

# 2. Install the template
dotnet new install ./Services/HakuService

# 3. Verify installation
dotnet new list | grep microservice
# Should show: Enterprise Microservice  microservice  [C#]
```

### NuGet Authentication (Required for Private Packages)

If packages are private, developers need to authenticate:

```bash
# Add GitHub Packages source with authentication
dotnet nuget add source "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
  --name "github-pts" \
  --username "YOUR_GITHUB_USERNAME" \
  --password "YOUR_GITHUB_TOKEN" \
  --store-password-in-clear-text
```

**Note:** GitHub token needs `read:packages` scope.

---

## 🏗️ Creating a New Microservice

### Step 1: Create Service

```bash
# Navigate to your projects directory
cd ~/projects

# Create new service
dotnet new microservice \
  --serviceName OrderService \
  --entityName Order \
  --entityPlural Orders \
  --port 5001 \
  --databaseName orders_db
```

### Step 2: Initialize Git Repository

```bash
cd OrderService
git init
git add .
git commit -m "Initial commit from microservice template"

# Create repo on GitHub and push
git remote add origin https://github.com/property-technology-solutions/order-service.git
git push -u origin main
```

### Step 3: Database Migration

```bash
cd src/OrderService.API

# Create initial migration
dotnet ef migrations add InitialCreate --project ../OrderService.Infrastructure

# Apply migration (requires running PostgreSQL)
dotnet ef database update
```

### Step 4: Configure Application

Edit `src/OrderService.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=orders_db;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "UseKeycloak": false,
  "Keycloak": {
    "Authority": "https://keycloak.company.com/realms/platform",
    "ClientId": "order-service"
  }
}
```

### Step 5: Run Service

```bash
# Start infrastructure
docker-compose up -d postgres redis

# Run service
dotnet run --project src/OrderService.API

# Access
# Swagger: http://localhost:5001/swagger
# Health: http://localhost:5001/health
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions Example

Create `.github/workflows/ci.yml` in your service repo:

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Configure NuGet
        run: |
          dotnet nuget add source "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
            --name "github-pts" \
            --username ${{ github.actor }} \
            --password ${{ secrets.GITHUB_TOKEN }} \
            --store-password-in-clear-text
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore -c Release
      
      - name: Test
        run: dotnet test --no-build -c Release
```

---

## 🐳 Docker Deployment

### Dockerfile (Already Included)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... (see generated Dockerfile)
```

### Docker Compose (Production)

```yaml
version: '3.8'
services:
  order-service:
    build:
      context: ./order-service
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=orders_db;...
      - ConnectionStrings__Redis=redis:6379
    ports:
      - "5001:8080"
    depends_on:
      - postgres
      - redis
```

---

## 📊 Monitoring

### Health Checks

Each service exposes:

| Endpoint | Purpose |
|----------|---------|
| `/health` | Overall health status |
| `/health/live` | Kubernetes liveness probe |
| `/health/ready` | Kubernetes readiness probe |

### Prometheus Metrics

Metrics available at `/metrics`:

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'order-service'
    static_configs:
      - targets: ['order-service:8080']
    metrics_path: '/metrics'
```

### OpenTelemetry Tracing

Configure in `appsettings.json`:

```json
{
  "OpenTelemetry": {
    "Endpoint": "http://otel-collector:4317"
  }
}
```

---

## 🔐 Security

### Keycloak Integration

1. Set `UseKeycloak: true` in appsettings
2. Configure Keycloak settings:

```json
{
  "UseKeycloak": true,
  "Keycloak": {
    "Authority": "https://keycloak.company.com/realms/platform",
    "ClientId": "order-service",
    "ClientSecret": "${KEYCLOAK_SECRET}",
    "RequireHttpsMetadata": true
  }
}
```

3. Use `[Authorize]` attribute on controllers

### Secrets Management

**Development:**
- Use `appsettings.Development.json` (gitignored)
- Use user secrets: `dotnet user-secrets set "Key" "Value"`

**Production:**
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Kubernetes Secrets

---

## 📦 Updating BuildingBlocks

When a new BuildingBlocks version is released:

```bash
cd your-service

# Update packages to new version
dotnet add package Enterprise.BuildingBlocks.Domain --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.Application --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.Infrastructure --version 2.2.0
dotnet add package Enterprise.BuildingBlocks.API --version 2.2.0

# Or edit Directory.Packages.props and run:
dotnet restore
```

---

## ❓ FAQ

### Q: Do I need to clone microservice-template for every new service?

**A:** No! You only need to:
1. Clone once to install the template
2. After that, run `dotnet new microservice` anywhere

### Q: How do services get BuildingBlocks updates?

**A:** Via NuGet package updates. Services are independent and only reference BuildingBlocks packages.

### Q: Can I modify BuildingBlocks for my service?

**A:** No. BuildingBlocks changes should be made in the template repo and released as new versions. This ensures consistency across all services.

### Q: What if I need a feature not in BuildingBlocks?

**A:** 
1. For service-specific features: Add to your service's codebase
2. For reusable features: Submit PR to microservice-template repo

---

## 🆘 Support

- **Repository Issues:** https://github.com/property-technology-solutions/microservice-template/issues
- **Slack Channel:** #platform-team
- **Wiki:** https://wiki.company.com/microservices
