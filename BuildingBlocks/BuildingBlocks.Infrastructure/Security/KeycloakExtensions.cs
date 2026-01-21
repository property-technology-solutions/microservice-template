using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Extension methods for Keycloak authentication and authorization setup.
/// Uses official Keycloak.AuthServices NuGet packages for robust OIDC integration.
/// 
/// Configuration format (appsettings.json):
/// "Keycloak": {
///   "AuthServerUrl": "https://keycloak.example.com",
///   "Realm": "MyRealm",
///   "Resource": "my-api-client",
///   "VerifyTokenAudience": true
/// }
/// </summary>
public static class KeycloakExtensions
{
    /// <summary>
    /// Adds Keycloak JWT Bearer authentication.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when Keycloak configuration section is missing</exception>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ValidateKeycloakConfiguration(configuration);

        services.AddKeycloakWebApiAuthentication(
            configuration,
            ConfigureJwtBearerOptions);

        return services;
    }

    /// <summary>
    /// Adds Keycloak JWT Bearer authentication with custom JWT Bearer options.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="configureOptions">Custom JWT Bearer options configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions> configureOptions)
    {
        ValidateKeycloakConfiguration(configuration);

        services.AddKeycloakWebApiAuthentication(
            configuration,
            options =>
            {
                ConfigureJwtBearerOptions(options);
                configureOptions(options);
            });

        return services;
    }

    /// <summary>
    /// Adds Keycloak authorization with role-based access control.
    /// Maps realm_access.roles and resource_access.{client}.roles to ClaimTypes.Role.
    /// Enables [Authorize(Roles = "role-name")] attribute usage.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKeycloakRoleAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Keycloak.AuthServices.Authorization maps Keycloak roles to ClaimTypes.Role
        services.AddKeycloakAuthorization(configuration);
        return services;
    }

    /// <summary>
    /// Adds both Keycloak authentication and authorization in one call.
    /// Recommended for most microservices.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKeycloakAuthenticationAndAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKeycloakAuthentication(configuration);
        services.AddKeycloakRoleAuthorization(configuration);
        return services;
    }

    private static void ValidateKeycloakConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(KeycloakAuthOptions.SectionName);
        
        if (!section.Exists())
        {
            throw new InvalidOperationException(
                $"Keycloak configuration section '{KeycloakAuthOptions.SectionName}' not found. " +
                "Expected configuration: Keycloak:AuthServerUrl, Keycloak:Realm, Keycloak:Resource");
        }

        var authServerUrl = section["AuthServerUrl"];
        var realm = section["Realm"];
        var resource = section["Resource"];

        if (string.IsNullOrWhiteSpace(authServerUrl))
            throw new InvalidOperationException("Keycloak:AuthServerUrl is required");
        
        if (string.IsNullOrWhiteSpace(realm))
            throw new InvalidOperationException("Keycloak:Realm is required");
        
        if (string.IsNullOrWhiteSpace(resource))
            throw new InvalidOperationException("Keycloak:Resource is required");
    }

    private static void ConfigureJwtBearerOptions(JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                var userName = context.Principal?.Identity?.Name ?? "anonymous";
                var roles = context.Principal?.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles")
                    .Select(c => c.Value)
                    .ToList() ?? [];

                logger?.LogDebug(
                    "Token validated for user {UserName} with {RoleCount} roles: [{Roles}]",
                    userName,
                    roles.Count,
                    string.Join(", ", roles));

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                
                logger?.LogWarning(
                    context.Exception,
                    "Authentication failed: {ExceptionType} - {Message}",
                    context.Exception.GetType().Name,
                    context.Exception.Message);

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                
                logger?.LogDebug(
                    "Authentication challenge for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                // SignalR/WebSocket token from query string support
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    }
}
