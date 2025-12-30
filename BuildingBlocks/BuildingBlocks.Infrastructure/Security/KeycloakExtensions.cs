using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Extension methods for Keycloak authentication setup
/// </summary>
public static class KeycloakExtensions
{
    /// <summary>
    /// Add Keycloak authentication with RBAC support
    /// Configures JWT Bearer authentication with Keycloak
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakOptions = configuration.GetSection(KeycloakAuthOptions.SectionName)
            .Get<KeycloakAuthOptions>() ?? throw new InvalidOperationException("Keycloak configuration not found");

        // Clear default claim type mappings
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keycloak issuer URL
                options.Authority = $"{keycloakOptions.Authority}/realms/{keycloakOptions.Realm}";
                options.Audience = keycloakOptions.ClientId;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = keycloakOptions.ValidateAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = $"{keycloakOptions.Authority}/realms/{keycloakOptions.Realm}",
                    ValidAudience = keycloakOptions.ClientId,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    
                    // Role claim type for Keycloak
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Extract roles from Keycloak token
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity != null)
                        {
                            // Keycloak stores roles in "realm_access.roles" or "resource_access.{client}.roles"
                            var token = context.SecurityToken as JwtSecurityToken;
                            if (token != null)
                            {
                                // Extract realm roles
                                var realmAccessClaim = token.Claims
                                    .FirstOrDefault(c => c.Type == "realm_access");
                                
                                if (realmAccessClaim != null && !string.IsNullOrEmpty(realmAccessClaim.Value))
                                {
                                    var realmAccess = System.Text.Json.JsonSerializer
                                        .Deserialize<RealmAccess>(realmAccessClaim.Value);
                                    
                                    if (realmAccess?.Roles != null)
                                    {
                                        foreach (var role in realmAccess.Roles)
                                        {
                                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                        }
                                    }
                                }

                                // Extract client roles
                                var resourceAccessClaim = token.Claims
                                    .FirstOrDefault(c => c.Type == "resource_access");
                                
                                if (resourceAccessClaim != null && !string.IsNullOrEmpty(resourceAccessClaim.Value))
                                {
                                    var resourceAccess = System.Text.Json.JsonSerializer
                                        .Deserialize<Dictionary<string, ClientAccess>>(resourceAccessClaim.Value);
                                    
                                    if (resourceAccess != null && 
                                        resourceAccess.TryGetValue(keycloakOptions.ClientId, out var clientAccess) &&
                                        clientAccess.Roles != null)
                                    {
                                        foreach (var role in clientAccess.Roles)
                                        {
                                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                        }
                                    }
                                }
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Authentication failed",
                            message = context.Exception.Message
                        });

                        return context.Response.WriteAsync(result);
                    }
                };
            });

        return services;
    }

    private class RealmAccess
    {
        public List<string>? Roles { get; set; }
    }

    private class ClientAccess
    {
        public List<string>? Roles { get; set; }
    }
}

