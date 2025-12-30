namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Keycloak authentication configuration options
/// </summary>
public class KeycloakAuthOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// Keycloak server URL (e.g., https://keycloak.example.com)
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Realm name in Keycloak
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Client ID registered in Keycloak
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret (for confidential clients)
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Whether to require HTTPS metadata
    /// Set to false for development
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Validate audience in JWT token
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Role claim type in JWT token
    /// Default: "realm_access.roles" for Keycloak
    /// </summary>
    public string RoleClaimType { get; set; } = "realm_access.roles";
}

