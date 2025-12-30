# Changelog

All notable changes to this project will be documented in this file.

## [2.1.0] - 2025-12-30

### 🎉 Enterprise Upgrade

### ✅ Added
- **Generic Repository Pattern** - `IRepository<T>` + `IReadRepository<T>` interfaces
- **RFC 7807 Problem Details** - Standardized error responses
- **API Response Wrapper** - `ApiResponse<T>` for consistent success responses
- **Input Sanitization** - XSS protection middleware
- **Audit Interceptor** - Auto-populate CreatedBy/UpdatedBy/Created/Updated
- **Global Query Filters** - Soft delete + Multi-tenancy (SSId) filters
- **Base Controller** - `BaseApiController` with helper methods
- **Feature Flags** - Runtime feature toggles with percentage rollouts
- **API Documentation** - XML comments integrated with Swagger

### 🔧 Changed
- Controllers now inherit from `BaseApiController`
- Centralized DI registration via extension methods
- Improved Swagger documentation with contact/license info
- OpenTelemetry packages updated to latest versions

### 📚 Documentation
- Consolidated all docs into single `README.md`
- Added developer usage guide
- Removed redundant documentation files

### ❌ Removed
- `docs/ARCHITECTURE.md` (merged into README)
- `docs/GETTING_STARTED.md` (merged into README)
- `TEMPLATE_USAGE.md` (merged into README)

---

## [2.0.0] - 2025-12-29

### 🎉 Major Changes
- **BREAKING**: Migrated from Event-Driven to HTTP-Only architecture
- **NEW**: Multi-language support with Translation Table pattern
- **NEW**: HTTP service communication with IServiceClient
- **NEW**: Language middleware with Accept-Language header support

### ✅ Added
- `ITranslatable` interface for entities with translations
- `BaseTranslation` base class for translation entities
- `HakuTranslation` entity for multi-language content
- `ILanguageService` for language detection
- `LanguageService` implementation with Accept-Language parsing
- `LanguageMiddleware` for automatic language detection
- `IServiceClient` interface for service-to-service communication
- `ServiceClient` implementation with Polly resilience
- Supported languages configuration (tr, en, ar, de, fr)
- Service URLs configuration for inter-service calls

### ❌ Removed
- RabbitMQ message broker
- MassTransit library and configuration
- Outbox Pattern implementation
- Integration Events
- Domain Event Interceptor
- `BuildingBlocks.Messaging` project
- All MassTransit-related dependencies
- RabbitMQ Docker service from docker-compose.yml

### 🔧 Changed
- `ApplicationDbContext` simplified (no more MassTransit Outbox)
- `Program.cs` updated with HTTP client and language service
- Health checks updated (removed RabbitMQ check)
- `docker-compose.yml` updated (removed RabbitMQ service)
- `Directory.Packages.props` updated (removed MassTransit packages)
- OpenTelemetry configuration (removed MassTransit tracing)

### 📚 Documentation
- Updated `README.md` with HTTP-only architecture
- Added `CHANGELOG.md` for version tracking

---

## [1.0.0] - 2025-12-29

### 🎉 Initial Release
- Clean Architecture implementation
- CQRS with MediatR
- Event-Driven Architecture with MassTransit + RabbitMQ
- PostgreSQL with EF Core 9
- Redis distributed cache
- JWT Authentication
- OpenTelemetry + Prometheus
- Health Checks
- FluentValidation
- Docker + docker-compose
- Nginx API Gateway
- `dotnet new` template support
- Shell script for service creation
- Comprehensive documentation

---

## Migration Notes

**From 2.0.0 to 2.1.0:**
- No breaking changes
- Update controller inheritance to `BaseApiController`
- Add `AddBuildingBlocksApi()` to Program.cs
- Configure feature flags in appsettings.json (optional)

**From 1.0.0 to 2.0.0:**
- Remove MassTransit dependencies
- Add multi-language support
- Update service communication to HTTP
- Update docker-compose (remove RabbitMQ)

