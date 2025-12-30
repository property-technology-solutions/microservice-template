# 🚀 Enterprise Microservice Template

> Production-ready .NET 9.0 microservice template with Clean Architecture, CQRS, DDD, and enterprise patterns.

---

## ✨ Features

| Category | Features |
|----------|----------|
| **Architecture** | Clean Architecture, CQRS, DDD, Vertical Slice |
| **Patterns** | Repository, Specification, Unit of Work, Result |
| **API** | RFC 7807 Problem Details, Versioning, Rate Limiting |
| **Security** | JWT, Keycloak-ready, XSS Protection, Role-based Auth |
| **Observability** | OpenTelemetry, Prometheus, Serilog, Health Checks |
| **Resilience** | Polly (Retry + Circuit Breaker), Redis Cache |
| **DevOps** | Docker, Nginx, Hangfire Background Jobs |
| **Multi-tenancy** | SSId-based isolation, Global Query Filters |
| **Feature Flags** | Runtime feature toggles, Percentage rollouts |

---

## 🚀 Quick Start

### 1. Clone & Install Template

```bash
git clone https://github.com/your-org/microservice-template.git
cd microservice-template
dotnet new install .
```

### 2. Create New Service

```bash
dotnet new microservice \
  --service-name OrderService \
  --entity-name Order \
  --output ./Services/OrderService
```

### 3. Run

```bash
cd Services/OrderService/src/OrderService.API
dotnet ef migrations add InitialCreate --project ../OrderService.Infrastructure
docker-compose up -d postgres redis
dotnet run
```

### 4. Access

- **Swagger:** http://localhost:5000/swagger
- **Health:** http://localhost:5000/health
- **Metrics:** http://localhost:5000/metrics

---

## 📁 Project Structure

```
microservice-template/
├── BuildingBlocks/                    # Shared libraries
│   ├── BuildingBlocks.Domain/         # DDD base classes, Repository interfaces
│   ├── BuildingBlocks.Application/    # CQRS, Behaviors, Result pattern
│   ├── BuildingBlocks.Infrastructure/ # Cache, Resilience, Feature Flags
│   └── BuildingBlocks.API/            # Base controllers, Filters, Middleware
├── Services/
│   └── HakuService/                   # Example service
├── docker-compose.yml
└── README.md
```

---

## 📖 Developer Guide

### API Response Format

**Success Response:**
```json
{
  "success": true,
  "data": { "id": 1, "name": "Test" },
  "message": "Operation completed successfully",
  "timestamp": "2025-12-30T10:00:00Z",
  "traceId": "abc-123"
}
```

**Error Response (RFC 7807):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-3.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "traceId": "abc-123",
  "errors": { "Name": ["Name is required"] }
}
```

---

### Using Feature Flags

**Configuration (appsettings.json):**
```json
{
  "FeatureFlags": {
    "NewDashboard": true,
    "BetaFeature": false,
    "PremiumFeature": {
      "Enabled": true,
      "Percentage": 50,
      "AllowedTenants": [1, 2, 3],
      "AllowedRoles": ["Admin"]
    }
  }
}
```

**In Code:**
```csharp
// Inject service
private readonly IFeatureFlagService _featureFlags;

// Check flag
if (_featureFlags.IsEnabled("NewDashboard"))
{
    // New feature code
}

// Context-aware check
var context = new FeatureFlagContext { TenantId = 1, Role = "Admin" };
if (_featureFlags.IsEnabled("PremiumFeature", context))
{
    // Premium feature
}
```

**On Controllers:**
```csharp
[FeatureFlag("NewDashboard")]
[HttpGet("new-dashboard")]
public IActionResult GetNewDashboard() { ... }
```

---

### Using Repository Pattern

```csharp
// Inject repository
private readonly IRepository<Order> _orderRepository;

// Query with specification
var spec = new ActiveOrdersSpecification(customerId);
var orders = await _orderRepository.ListAsync(spec);

// Add entity
var order = Order.Create(customerId, items);
await _orderRepository.AddAsync(order);
await _unitOfWork.SaveChangesAsync();
```

---

### Creating Commands/Queries

**Command:**
```csharp
public record CreateOrderCommand(int CustomerId, List<OrderItem> Items) 
    : IRequest<Result<OrderResponse>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.CustomerId, request.Items);
        await _repository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrderResponse>.Success(order.ToResponse());
    }
}
```

**Validator:**
```csharp
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
    }
}
```

---

### Controller Best Practices

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly ISender _sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _sender.Send(command);
        
        if (result.IsFailure)
            return ApiBadRequest(result.Errors);
        
        return ApiCreated(result.Value!, nameof(GetById), new { id = result.Value!.Id });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _sender.Send(new GetOrderQuery(id));
        
        if (result.IsFailure)
            return ApiNotFound($"Order {id} not found");
        
        return ApiOk(result.Value!);
    }
}
```

---

## 🐳 Docker

```bash
# Start all infrastructure
docker-compose up -d

# Services:
# - PostgreSQL: localhost:5432
# - Redis: localhost:6379
# - Nginx: localhost:80
# - OpenTelemetry: localhost:4317
```

---

## 📋 Checklist for New Services

- [ ] Create service using template
- [ ] Run database migrations
- [ ] Configure appsettings.json
- [ ] Implement domain entities
- [ ] Add commands/queries
- [ ] Add validators
- [ ] Create specifications
- [ ] Test API endpoints
- [ ] Configure feature flags
- [ ] Add to docker-compose

---

---

## 🏢 Enterprise Deployment

Enterprise ortamda dağıtım için [DEPLOYMENT.md](./DEPLOYMENT.md) dokümanına bakın:

- Internal Git Repository kurulumu
- NuGet Feed entegrasyonu
- CI/CD Pipeline yapılandırması
- Developer onboarding
- BuildingBlocks versiyon yönetimi

---

## 📄 License

MIT License
