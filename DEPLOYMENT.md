# 🏢 Enterprise Deployment Guide

Bu doküman, template'in enterprise ortamda nasıl dağıtılacağını ve kullanılacağını açıklar.

---

## 📍 Nereye Koymalı?

### Seçenek 1: Internal Git Repository (Önerilen)

```
Azure DevOps / GitHub Enterprise / GitLab
└── microservice-template/
    ├── BuildingBlocks/          ← Shared libraries
    ├── Services/
    │   └── HakuService/         ← Template source
    ├── docker-compose.yml
    └── README.md
```

**Avantajlar:**
- Versiyon kontrolü
- PR/Code review
- CI/CD entegrasyonu
- Access control

---

### Seçenek 2: Internal NuGet Feed + Template Package

```
Azure Artifacts / GitHub Packages / Nexus
├── PropertyTech.BuildingBlocks.Domain
├── PropertyTech.BuildingBlocks.Application
├── PropertyTech.BuildingBlocks.Infrastructure
├── PropertyTech.BuildingBlocks.API
└── PropertyTech.Microservice.Template
```

**Avantajlar:**
- Bağımsız servis geliştirme
- Versiyon yönetimi
- Paket güncelleme kontrolü

---

## 🚀 Kurulum Senaryoları

### Senaryo A: Yeni Geliştirici Başlangıcı

```bash
# 1. Template repository'yi klonla
git clone https://git.company.com/platform/microservice-template.git
cd microservice-template

# 2. Template'i yükle
dotnet new install ./Services/HakuService

# 3. Kendi servisini oluştur
cd ../my-services
dotnet new microservice \
  --serviceName OrderService \
  --entityName Order \
  --port 5001

# 4. BuildingBlocks referanslarını ayarla
# (Local development için symlink veya copy)
```

---

### Senaryo B: NuGet Paketleri ile (Production)

```bash
# 1. NuGet source ekle (bir kez)
dotnet nuget add source "https://pkgs.dev.azure.com/company/_packaging/internal/nuget/v3/index.json" \
  --name "CompanyFeed" \
  --username "azure" \
  --password "YOUR_PAT"

# 2. Template'i NuGet'ten yükle
dotnet new install PropertyTech.Microservice.Template

# 3. Yeni servis oluştur
dotnet new microservice \
  --serviceName OrderService \
  --entityName Order \
  --useLocalBuildingBlocks false
```

---

## 📋 Developer Onboarding Checklist

### Ön Koşullar
- [ ] .NET 9.0 SDK kurulu
- [ ] Docker Desktop kurulu
- [ ] Git kurulu
- [ ] IDE (Rider / VS2022 / VS Code)
- [ ] Internal NuGet feed erişimi (varsa)

### İlk Kurulum
```bash
# 1. Repository'yi klonla
git clone https://git.company.com/platform/microservice-template.git

# 2. Dependency'leri restore et
cd microservice-template
dotnet restore

# 3. Build et
dotnet build

# 4. Infrastructure'ı başlat
docker-compose up -d postgres redis

# 5. Örnek servisi çalıştır
cd Services/HakuService/src/HakuService.API
dotnet run

# 6. Test et
curl http://localhost:5000/health
```

### Template Kurulumu
```bash
# Template'i yükle
dotnet new install ./Services/HakuService

# Kurulumu doğrula
dotnet new list | grep microservice
```

---

## 🏗️ Yeni Servis Oluşturma

### Adım 1: Servis Oluştur
```bash
dotnet new microservice \
  --serviceName OrderService \
  --entityName Order \
  --entityPlural Orders \
  --databaseName orderdb \
  --port 5001 \
  --output ./Services/OrderService
```

### Adım 2: Veritabanı Migration
```bash
cd Services/OrderService/src/OrderService.API

# Migration oluştur
dotnet ef migrations add InitialCreate \
  --project ../OrderService.Infrastructure \
  --startup-project .

# Veritabanını güncelle
dotnet ef database update
```

### Adım 3: docker-compose'a Ekle
```yaml
# docker-compose.yml'e ekle
orderservice:
  build:
    context: ./Services/OrderService
    dockerfile: Dockerfile
  container_name: orderservice-api
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__DefaultConnection: "Host=postgres;Database=orderdb;..."
    ConnectionStrings__Redis: "redis:6379"
  ports:
    - "5001:8080"
  depends_on:
    - postgres
    - redis
```

### Adım 4: Çalıştır
```bash
# Local
dotnet run

# Docker
docker-compose up -d orderservice
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions (Otomatik)
```
Push to main → Build → Test → Template Test → Security Scan
Push tag v* → Build → Test → Pack NuGet → Publish → Create Release
```

### Azure DevOps Pipeline (Örnek)
```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '9.0.x'
  
  - script: dotnet restore
  - script: dotnet build --configuration Release
  - script: dotnet test --configuration Release
  
  - task: NuGetCommand@2
    inputs:
      command: 'pack'
      packagesToPack: '**/BuildingBlocks.*/*.csproj'
      versioningScheme: 'byBuildNumber'
  
  - task: NuGetCommand@2
    inputs:
      command: 'push'
      publishVstsFeed: 'internal-feed'
```

---

## 📦 BuildingBlocks Güncelleme

### Versiyon Yükseltme
```bash
# 1. Version güncelle (tüm csproj'larda)
# Directory.Build.props veya her csproj'da <Version>2.2.0</Version>

# 2. CHANGELOG güncelle
# CHANGELOG.md'ye yeni version ekle

# 3. Tag oluştur ve push et
git tag v2.2.0
git push origin v2.2.0

# 4. CI/CD otomatik olarak:
#    - Build & test
#    - NuGet paketleri oluştur
#    - Internal feed'e publish et
#    - GitHub Release oluştur
```

### Servislerde Güncelleme
```bash
# NuGet kullanıyorsa
dotnet add package PropertyTech.BuildingBlocks.Domain --version 2.2.0

# Local reference kullanıyorsa
git pull  # Template repo'dan
```

---

## 🔐 Güvenlik

### Secrets Yönetimi
```bash
# Development
# appsettings.Development.json (git ignore)

# Production
# Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
```

### Keycloak Entegrasyonu
```json
{
  "UseKeycloak": true,
  "Keycloak": {
    "Authority": "https://keycloak.company.com/realms/platform",
    "ClientId": "order-service",
    "ClientSecret": "${KEYCLOAK_SECRET}"  // Environment variable
  }
}
```

---

## 📊 Monitoring

### Endpoints
| Endpoint | Açıklama |
|----------|----------|
| `/health` | Genel sağlık durumu |
| `/health/live` | Kubernetes liveness probe |
| `/health/ready` | Kubernetes readiness probe |
| `/metrics` | Prometheus metrics |
| `/swagger` | API documentation |

### Prometheus Config
```yaml
scrape_configs:
  - job_name: 'order-service'
    static_configs:
      - targets: ['orderservice:8080']
    metrics_path: '/metrics'
```

---

## ❓ SSS

### Template güncellendikten sonra mevcut servisler nasıl güncellenir?
> Manual migration gerekir. CHANGELOG.md'deki breaking changes'i kontrol edin.

### BuildingBlocks'ta bug fix yaptım, servislere nasıl yansır?
> NuGet kullanıyorsanız: version bump + package update
> Local reference kullanıyorsanız: git pull

### Yeni bir BuildingBlocks özelliği nasıl eklenir?
> 1. Feature branch oluştur
> 2. Implement et
> 3. PR aç
> 4. Review sonrası merge
> 5. Tag ve release

---

## 📞 Destek

- **Slack:** #platform-team
- **Wiki:** https://wiki.company.com/microservice-template
- **Issues:** https://git.company.com/platform/microservice-template/issues

