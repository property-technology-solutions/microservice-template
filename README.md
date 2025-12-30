# 🚀 Enterprise Microservice Template

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> Production-ready .NET 9.0 microservice template with Clean Architecture, CQRS, DDD, and enterprise patterns.

---

## ✨ Features

| Category | Features |
|----------|----------|
| **Architecture** | Clean Architecture, CQRS, DDD, Vertical Slice |
| **Patterns** | Generic Repository, Specification, Unit of Work, Result |
| **API** | RFC 7807 Problem Details, Versioning, Rate Limiting |
| **Security** | JWT, Keycloak-ready, XSS Protection, Role-based Auth |
| **Observability** | OpenTelemetry, Prometheus, Serilog, Health Checks |
| **Resilience** | Polly (Retry + Circuit Breaker), Redis Cache |
| **DevOps** | Docker, Nginx, Hangfire Background Jobs |
| **Multi-tenancy** | SSId-based isolation, Global Query Filters |
| **Feature Flags** | Runtime feature toggles, Percentage rollouts |

---

## 📦 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Git

### 1. Install Template

```bash
# Clone repository
git clone https://github.com/property-technology-solutions/microservice-template.git
cd microservice-template

# Install template
dotnet new install ./Services/HakuService
```

### 2. Create New Service

```bash
# Create in any directory (services are independent!)
cd ~/projects
dotnet new microservice --serviceName OrderService --entityName Order --port 5001
```

### 3. Build & Run

```bash
cd OrderService

# Build
dotnet build

# Run (requires PostgreSQL and Redis)
docker-compose up -d postgres redis
dotnet run --project src/OrderService.API
```

### 4. Access

- **Swagger:** http://localhost:5001/swagger
- **Health:** http://localhost:5001/health
- **Metrics:** http://localhost:5001/metrics

---

## 📁 Project Structure

```
microservice-template/
├── BuildingBlocks/                    # Shared NuGet packages
│   ├── BuildingBlocks.Domain/         # Base entities, Repository interfaces, Specifications
│   ├── BuildingBlocks.Application/    # CQRS, Behaviors, Result pattern, Unit of Work
│   ├── BuildingBlocks.Infrastructure/ # Cache, Resilience, Feature Flags, Keycloak
│   └── BuildingBlocks.API/            # Base controllers, Middleware, Filters
├── Services/
│   └── HakuService/                   # Template source (example service)
├── nupkgs/                            # Built NuGet packages
├── docker-compose.yml
└── README.md
```

### Generated Service Structure

```
OrderService/
├── src/
│   ├── OrderService.Domain/           # Entities, Events, Enums
│   ├── OrderService.Application/      # Commands, Queries, Validators, Specifications
│   ├── OrderService.Infrastructure/   # DbContext, Configurations, DI
│   └── OrderService.API/              # Controllers, Middleware, Program.cs
├── nuget.config                       # Points to GitHub Packages
├── Directory.Packages.props           # Central package versions
├── Dockerfile
└── OrderService.sln
```

---

## 🏗️ Template Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `--serviceName` | Service name (e.g., OrderService) | Required |
| `--entityName` | Main entity name (e.g., Order) | Required |
| `--entityPlural` | Plural form (e.g., Orders) | {entityName}s |
| `--port` | HTTP port | 5000 |
| `--databaseName` | PostgreSQL database name | {entityName}db |

**Example:**
```bash
dotnet new microservice \
  --serviceName PaymentService \
  --entityName Payment \
  --entityPlural Payments \
  --port 5002 \
  --databaseName payments_db
```

---

## 📖 Developer Guide

### What You Write (Business Logic Only)

When creating a new feature, developers only write:

| Layer | Files | Example |
|-------|-------|---------|
| **Domain** | Entity, Events | `Order.cs`, `OrderCreatedEvent.cs` |
| **Application** | Command, Handler, Validator | `CreateOrderCommand.cs`, `CreateOrderCommandHandler.cs`, `CreateOrderCommandValidator.cs` |
| **Application** | Query, Handler | `GetOrderQuery.cs`, `GetOrderQueryHandler.cs` |
| **Application** | Specification | `ActiveOrdersSpecification.cs` |
| **Infrastructure** | EF Configuration | `OrderConfiguration.cs` |
| **API** | Controller | `OrdersController.cs` |

### What's Already Done (BuildingBlocks)

- ✅ Base entities with audit fields
- ✅ Generic Repository + Unit of Work
- ✅ Specification pattern infrastructure
- ✅ Result pattern for error handling
- ✅ MediatR behaviors (Validation, Logging, Transaction)
- ✅ Global exception handling (RFC 7807)
- ✅ API response wrapper
- ✅ Input sanitization (XSS protection)
- ✅ Feature flags
- ✅ Redis caching
- ✅ Polly resilience policies
- ✅ OpenTelemetry tracing
- ✅ Health checks
- ✅ API versioning

---

### Code Examples

#### Entity (Domain)

```csharp
public class Order : BaseEntity, IAggregateRoot
{
    public string CustomerName { get; private set; }
    public decimal Total { get; private set; }

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order() { } // EF Core

    public static Order Create(string customerName, decimal total, int ssId)
    {
        var order = new Order 
        { 
            CustomerName = customerName, 
            Total = total, 
            SSId = ssId, 
            Status = 1 
        };
        order._domainEvents.Add(new OrderCreatedEvent(order.Id, customerName));
        return order;
    }
}
```

#### Command + Handler (Application)

```csharp
// Command
public record CreateOrderCommand(string CustomerName, decimal Total, int SSId) 
    : IRequest<Result<OrderResponse>>;

// Handler
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderResponse>>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.CustomerName, request.Total, request.SSId);
        
        await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result<OrderResponse>.Success(new OrderResponse(order.Id, order.CustomerName));
    }
}

// Validator (auto-executed via MediatR pipeline)
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Total).GreaterThan(0);
        RuleFor(x => x.SSId).GreaterThan(0);
    }
}
```

#### Controller (API)

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var result = await Sender.Send(command);
        
        return result.IsSuccess 
            ? ApiCreated(result.Value!, nameof(GetById), new { id = result.Value!.Id })
            : ApiBadRequest(result.Errors);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Sender.Send(new GetOrderQuery(id));
        
        return result.IsSuccess 
            ? ApiOk(result.Value!)
            : ApiNotFound($"Order {id} not found");
    }

    [HttpGet("featured")]
    [FeatureFlag("BetaFeatures")]  // Returns 404 if feature is disabled
    public async Task<IActionResult> GetFeatured() { ... }
}
```

---

### API Response Format

**Success Response:**
```json
{
  "success": true,
  "data": { "id": 1, "customerName": "John Doe" },
  "message": "Order created successfully",
  "timestamp": "2025-12-30T10:00:00Z",
  "traceId": "abc-123-def"
}
```

**Error Response (RFC 7807):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "traceId": "abc-123-def",
  "timestamp": "2025-12-30T10:00:00Z",
  "errors": {
    "CustomerName": ["Customer name is required"],
    "Total": ["Total must be greater than 0"]
  }
}
```

---

### Feature Flags

**Configuration (appsettings.json):**
```json
{
  "FeatureFlags": {
    "NewDashboard": true,
    "BetaFeatures": false,
    "ShowFeaturedFirst": true
  }
}
```

**In Handler:**
```csharp
private readonly IFeatureFlagService _featureFlags;

if (_featureFlags.IsEnabled("ShowFeaturedFirst"))
{
    // Feature-specific logic
}
```

**In Controller (Attribute):**
```csharp
[FeatureFlag("BetaFeatures")]
[HttpGet("beta-endpoint")]
public IActionResult BetaEndpoint() { ... }
```

---

## 🐳 Docker

```bash
# Start all infrastructure
docker-compose up -d

# Services:
# - PostgreSQL: localhost:5432
# - Redis: localhost:6379
# - Nginx: localhost:80 (reverse proxy)
# - OpenTelemetry Collector: localhost:4317
```

---

## 📦 NuGet Packages

Services consume BuildingBlocks via NuGet packages from GitHub Packages:

| Package | Description |
|---------|-------------|
| `Enterprise.BuildingBlocks.Domain` | Base entities, Repository interfaces, Specifications |
| `Enterprise.BuildingBlocks.Application` | CQRS, Behaviors, Result pattern |
| `Enterprise.BuildingBlocks.Infrastructure` | Cache, Resilience, Feature Flags |
| `Enterprise.BuildingBlocks.API` | Controllers, Middleware, Filters |

**Feed URL:** `https://nuget.pkg.github.com/property-technology-solutions/index.json`

---

## 🔄 Releasing New BuildingBlocks Version

See [RELEASING.md](./RELEASING.md) for detailed instructions.

**Quick Summary:**
```bash
VERSION=2.2.0

# 1. Pack
dotnet pack BuildingBlocks/BuildingBlocks.API/BuildingBlocks.API.csproj -o nupkgs -c Release /p:Version=$VERSION

# 2. Push to GitHub Packages
dotnet nuget push "nupkgs/*.$VERSION.nupkg" \
  -s "https://nuget.pkg.github.com/property-technology-solutions/index.json" \
  -k $GITHUB_TOKEN

# 3. Update template versions
# Edit Services/HakuService/Directory.Packages.props

# 4. Commit & tag
git add . && git commit -m "Release v$VERSION" && git push
git tag v$VERSION && git push origin v$VERSION
```

---

## 📋 Checklist for New Services

- [ ] Create service: `dotnet new microservice --serviceName X --entityName Y`
- [ ] Create database migration: `dotnet ef migrations add InitialCreate`
- [ ] Update `appsettings.json` (connection strings, Keycloak, etc.)
- [ ] Implement domain entities
- [ ] Add commands/queries with validators
- [ ] Create specifications for complex queries
- [ ] Test API endpoints via Swagger
- [ ] Configure feature flags if needed
- [ ] Add service to docker-compose (production)

---

## 📚 Additional Documentation

- [RELEASING.md](./RELEASING.md) - How to release new BuildingBlocks versions
- [DEPLOYMENT.md](./DEPLOYMENT.md) - Enterprise deployment guide
- [CHANGELOG.md](./CHANGELOG.md) - Version history

---

## 🤝 Contributing

1. Create feature branch from `main`
2. Make changes to BuildingBlocks
3. Test with a new service
4. Create PR
5. After merge, follow release process

---

## 📄 License

MIT License - see [LICENSE](./LICENSE)

---

## 🆘 Support

- **Issues:** https://github.com/property-technology-solutions/microservice-template/issues
- **Slack:** #platform-team
