using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.FeatureFlags;

/// <summary>
/// Configuration-based feature flag service.
/// Reads feature flags from appsettings.json.
/// 
/// Configuration format:
/// {
///   "FeatureFlags": {
///     "NewDashboard": true,
///     "BetaFeature": false,
///     "PremiumFeature": {
///       "Enabled": true,
///       "Percentage": 50,
///       "AllowedTenants": [1, 2, 3]
///     }
///   }
/// }
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly Dictionary<string, FeatureFlagConfig> _featureConfigs;

    public FeatureFlagService(IConfiguration configuration, ILogger<FeatureFlagService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _featureConfigs = LoadFeatureConfigs();
    }

    /// <inheritdoc/>
    public bool IsEnabled(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
            return false;

        if (_featureConfigs.TryGetValue(featureName, out var config))
        {
            return config.Enabled;
        }

        // Try simple boolean from config
        var simpleValue = _configuration[$"FeatureFlags:{featureName}"];
        if (bool.TryParse(simpleValue, out var enabled))
        {
            return enabled;
        }

        _logger.LogDebug("Feature flag '{FeatureName}' not found, defaulting to false", featureName);
        return false;
    }

    /// <inheritdoc/>
    public Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsEnabled(featureName));
    }

    /// <inheritdoc/>
    public bool IsEnabled(string featureName, FeatureFlagContext context)
    {
        if (string.IsNullOrWhiteSpace(featureName))
            return false;

        if (!_featureConfigs.TryGetValue(featureName, out var config))
        {
            return IsEnabled(featureName);
        }

        // Check if feature is globally disabled
        if (!config.Enabled)
            return false;

        // Check tenant-specific allowlist
        if (config.AllowedTenants?.Count > 0 && context.TenantId.HasValue)
        {
            if (!config.AllowedTenants.Contains(context.TenantId.Value))
            {
                return false;
            }
        }

        // Check role-based access
        if (config.AllowedRoles?.Count > 0 && !string.IsNullOrEmpty(context.Role))
        {
            if (!config.AllowedRoles.Contains(context.Role, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Check percentage rollout
        if (config.Percentage.HasValue && config.Percentage < 100)
        {
            if (string.IsNullOrEmpty(context.UserId))
                return false;

            var hash = Math.Abs(context.UserId.GetHashCode()) % 100;
            return hash < config.Percentage;
        }

        return true;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, bool> GetAllFeatures()
    {
        var features = new Dictionary<string, bool>();

        foreach (var config in _featureConfigs)
        {
            features[config.Key] = config.Value.Enabled;
        }

        // Also include simple boolean features
        var section = _configuration.GetSection("FeatureFlags");
        foreach (var child in section.GetChildren())
        {
            if (!features.ContainsKey(child.Key) && bool.TryParse(child.Value, out var enabled))
            {
                features[child.Key] = enabled;
            }
        }

        return features;
    }

    private Dictionary<string, FeatureFlagConfig> LoadFeatureConfigs()
    {
        var configs = new Dictionary<string, FeatureFlagConfig>(StringComparer.OrdinalIgnoreCase);
        var section = _configuration.GetSection("FeatureFlags");

        foreach (var child in section.GetChildren())
        {
            // Check if it's a complex config (has sub-properties)
            var enabledValue = child["Enabled"];
            if (enabledValue != null)
            {
                var config = new FeatureFlagConfig
                {
                    Enabled = bool.TryParse(enabledValue, out var e) && e,
                    Percentage = int.TryParse(child["Percentage"], out var p) ? p : null,
                    AllowedTenants = child.GetSection("AllowedTenants").Get<List<int>>(),
                    AllowedRoles = child.GetSection("AllowedRoles").Get<List<string>>()
                };
                configs[child.Key] = config;
            }
        }

        _logger.LogInformation("Loaded {Count} feature flag configurations", configs.Count);
        return configs;
    }
}

/// <summary>
/// Internal configuration model for complex feature flags.
/// </summary>
internal class FeatureFlagConfig
{
    public bool Enabled { get; set; }
    public int? Percentage { get; set; }
    public List<int>? AllowedTenants { get; set; }
    public List<string>? AllowedRoles { get; set; }
}

