namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Provides access to current authenticated user information
/// Extracted from JWT token claims
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Current user's unique identifier
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Shopping Center ID for multi-tenancy
    /// </summary>
    int? SSId { get; }

    /// <summary>
    /// User's role (Admin, Manager, User, etc.)
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Whether user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Whether user has admin privileges
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Get specific claim value
    /// </summary>
    string? GetClaim(string claimType);
}

