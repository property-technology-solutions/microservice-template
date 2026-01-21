namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Keycloak authentication configuration options.
/// Compatible with Keycloak.AuthServices.Authentication package.
/// 
/// appsettings.json example:
/// "Keycloak": {
///   "AuthServerUrl": "https://keycloak.example.com",
///   "Realm": "MyRealm",
///   "Resource": "my-api-client",
///   "VerifyTokenAudience": true,
///   "Credentials": { "Secret": "client-secret" }
/// }
/// </summary>
public class KeycloakAuthOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// Keycloak server base URL (e.g., https://keycloak.example.com)
    /// </summary>
    public string AuthServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Keycloak realm name
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Client ID (resource) registered in Keycloak
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Verify token audience claim. Default: true
    /// </summary>
    public bool VerifyTokenAudience { get; set; } = true;

    /// <summary>
    /// SSL requirement level: "none", "external", "all". Default: external
    /// </summary>
    public string SslRequired { get; set; } = "external";

    /// <summary>
    /// Token clock skew tolerance for validation
    /// </summary>
    public TimeSpan TokenClockSkew { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether this is a confidential client (has client secret)
    /// </summary>
    public bool ConfidentialPort { get; set; }

    /// <summary>
    /// Client credentials configuration
    /// </summary>
    public KeycloakCredentials? Credentials { get; set; }
}

/// <summary>
/// Keycloak client credentials for confidential clients
/// </summary>
public class KeycloakCredentials
{
    /// <summary>
    /// Client secret for confidential clients
    /// </summary>
    public string? Secret { get; set; }
}
