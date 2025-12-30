namespace BuildingBlocks.Infrastructure.FeatureFlags;

/// <summary>
/// Service for checking feature flag status.
/// Enables runtime feature toggles without deployment.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    /// <param name="featureName">Feature flag name</param>
    /// <returns>True if enabled</returns>
    bool IsEnabled(string featureName);

    /// <summary>
    /// Checks if a feature is enabled asynchronously.
    /// </summary>
    /// <param name="featureName">Feature flag name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if enabled</returns>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a feature is enabled for a specific context (e.g., user, tenant).
    /// </summary>
    /// <param name="featureName">Feature flag name</param>
    /// <param name="context">Context for evaluation (userId, tenantId, etc.)</param>
    /// <returns>True if enabled</returns>
    bool IsEnabled(string featureName, FeatureFlagContext context);

    /// <summary>
    /// Gets all feature flags and their status.
    /// </summary>
    /// <returns>Dictionary of feature names and their enabled status</returns>
    IReadOnlyDictionary<string, bool> GetAllFeatures();
}

/// <summary>
/// Context for feature flag evaluation.
/// Allows percentage rollouts and targeted releases.
/// </summary>
public class FeatureFlagContext
{
    /// <summary>
    /// User identifier for user-specific features.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Tenant identifier for tenant-specific features.
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// User role for role-based features.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Additional properties for custom evaluation.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
}

