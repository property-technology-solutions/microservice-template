using BuildingBlocks.API.Extensions;
using BuildingBlocks.API.Middleware;
using BuildingBlocks.API.RateLimiting;
using BuildingBlocks.API.Versioning;
using BuildingBlocks.Application.BackgroundJobs;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.BackgroundJobs;
using BuildingBlocks.Infrastructure.Cache;
using BuildingBlocks.Infrastructure.Http;
using BuildingBlocks.Infrastructure.Localization;
using BuildingBlocks.Infrastructure.Resilience;
using BuildingBlocks.Infrastructure.Security;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using HakuService.API.Middleware;
using HakuService.API.Services;
using HakuService.Application.Common.Interfaces;
using HakuService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERILOG CONFIGURATION
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("logs/haku-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================================================
// OPENTELEMETRY CONFIGURATION
// ============================================================================
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("HakuService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("HakuService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

// ============================================================================
// INFRASTRUCTURE SERVICES (Database, Repositories, Audit Interceptor)
// ============================================================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add HakuService Infrastructure (DbContext, UnitOfWork, Repositories, Audit)
builder.Services.AddHakuServiceInfrastructure(builder.Configuration);

// ============================================================================
// REDIS CONFIGURATION
// ============================================================================
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// ============================================================================
// HTTP CLIENT CONFIGURATION (Service-to-Service Communication)
// ============================================================================
builder.Services.AddHttpClient("ResilientHttpClient");
builder.Services.AddSingleton(ResiliencePolicies.GetRetryPolicy());
builder.Services.AddSingleton(ResiliencePolicies.GetCircuitBreakerPolicy());

builder.Services.AddScoped<IServiceClient, ServiceClient>();

// ============================================================================
// LOCALIZATION CONFIGURATION
// ============================================================================
builder.Services.AddScoped<ILanguageService, LanguageService>();

// ============================================================================
// MEDIATR & VALIDATION
// ============================================================================
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly);
    
    // Add pipeline behaviors
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(IApplicationDbContext).Assembly);

// ============================================================================
// AUTHENTICATION & AUTHORIZATION
// ============================================================================
var useKeycloak = builder.Configuration.GetValue<bool>("UseKeycloak", false);

if (useKeycloak)
{
    // Keycloak authentication with RBAC
    builder.Services.AddKeycloakAuthentication(builder.Configuration);
    Log.Information("Using Keycloak authentication");
}
else
{
    // Simple JWT authentication (development/testing)
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(secretKey)
            };
        });
    Log.Information("Using simple JWT authentication");
}

builder.Services.AddAuthorization(options =>
{
    // Define role-based policies
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Manager", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("User", policy => policy.RequireRole("Admin", "Manager", "User"));
});

// ============================================================================
// HANGFIRE BACKGROUND JOBS
// ============================================================================
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = new[] { "default", "critical", "background" };
});

builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

// ============================================================================
// API VERSIONING
// ============================================================================
builder.Services.AddApiVersioningConfiguration();

// ============================================================================
// API CONFIGURATION (with BuildingBlocks.API response formatting)
// ============================================================================
builder.Services.AddBuildingBlocksApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Haku Service API",
        Version = "v1",
        Description = "Enterprise microservice for managing Hakus with Clean Architecture, CQRS, and DDD patterns.",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments from all assemblies
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
    {
        c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
    }

    // Add JWT authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================================
// HEALTH CHECKS
// ============================================================================
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "", name: "database")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", "redis");

// ============================================================================
// CORS (Managed by Nginx in production)
// ============================================================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================================
// BUILD & CONFIGURE MIDDLEWARE PIPELINE
// ============================================================================
var app = builder.Build();

// Global exception handling (RFC 7807 compliant)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Input sanitization (XSS protection)
app.UseInputSanitization();

// Correlation ID for distributed tracing
app.UseMiddleware<CorrelationIdMiddleware>();

// Language detection
app.UseMiddleware<LanguageMiddleware>();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Hangfire Dashboard (development only)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = Array.Empty<Hangfire.Dashboard.IDashboardAuthorizationFilter>() // Dev only!
    });
}

// Rate limiting (100 requests per minute per user/IP)
app.UseSimpleRateLimiting(requestLimit: 100, timeWindowSeconds: 60);

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Prometheus metrics
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

// Controllers
app.MapControllers();

// ============================================================================
// RUN APPLICATION
// ============================================================================
try
{
    Log.Information("Starting HakuService API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}
