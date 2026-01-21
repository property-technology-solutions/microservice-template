using BuildingBlocks.API.Filters;
using BuildingBlocks.API.Middleware;
using BuildingBlocks.API.Services;
using BuildingBlocks.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.API.Extensions;

/// <summary>
/// Extension methods for registering BuildingBlocks.API services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds standard API response formatting including:
    /// - API response wrapper filter
    /// - Problem details configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApiResponseFormatting(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiResponseWrapperFilter>();
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            // Disable default model state validation response
            // We handle it in ValidationBehavior + GlobalExceptionMiddleware
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

    /// <summary>
    /// Adds ICurrentUserService for extracting user info from JWT claims.
    /// Required for AuditableEntityInterceptor.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }

    /// <summary>
    /// Adds all BuildingBlocks.API services including:
    /// - API response formatting
    /// - Current user service (for audit trail)
    /// - Standard configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBuildingBlocksApi(this IServiceCollection services)
    {
        services.AddApiResponseFormatting();
        services.AddCurrentUserService();
        return services;
    }

    /// <summary>
    /// Adds Keycloak authentication, authorization, and standard RBAC policies.
    /// One-liner setup for Keycloak-protected microservices.
    /// 
    /// Required appsettings.json configuration:
    /// "Keycloak": {
    ///   "AuthServerUrl": "https://keycloak.example.com",
    ///   "Realm": "your-realm",
    ///   "Resource": "your-client-id"
    /// }
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="configurePolicies">Optional custom authorization policies</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKeycloakSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthorizationOptions>? configurePolicies = null)
    {
        // Add Keycloak authentication (JWT Bearer)
        services.AddKeycloakAuthentication(configuration);

        // Add Keycloak role authorization (maps realm_access.roles & resource_access.{client}.roles to ClaimTypes.Role)
        services.AddKeycloakRoleAuthorization(configuration);

        // Add authorization with default policies
        services.AddAuthorization(options =>
        {
            // Standard role-based policies
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin", "Administrator"));
            options.AddPolicy("Manager", policy => policy.RequireRole("Admin", "Administrator", "Manager"));
            options.AddPolicy("User", policy => policy.RequireRole("Admin", "Administrator", "Manager", "User"));
            
            // Read-only policy
            options.AddPolicy("ReadOnly", policy => policy.RequireRole("ReadOnlyUser", "User", "Manager", "Admin", "Administrator"));

            // Apply custom policies if provided
            configurePolicies?.Invoke(options);
        });

        return services;
    }
}

/// <summary>
/// Extension methods for configuring BuildingBlocks.API middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds input sanitization middleware to the pipeline.
    /// Should be added early in the pipeline before request processing.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="options">Optional sanitization options</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseInputSanitization(
        this IApplicationBuilder app,
        InputSanitizationOptions? options = null)
    {
        return app.UseMiddleware<InputSanitizationMiddleware>(options ?? new InputSanitizationOptions());
    }

    /// <summary>
    /// Adds all BuildingBlocks.API middleware in the correct order.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseBuildingBlocksApi(this IApplicationBuilder app)
    {
        app.UseInputSanitization();
        return app;
    }
}

