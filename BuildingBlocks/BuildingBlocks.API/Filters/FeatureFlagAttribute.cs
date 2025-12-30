using BuildingBlocks.Infrastructure.FeatureFlags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.API.Filters;

/// <summary>
/// Attribute to gate controller actions behind feature flags.
/// Returns 404 Not Found if feature is disabled.
/// 
/// Usage:
/// [FeatureFlag("NewDashboard")]
/// public IActionResult GetNewDashboard() { ... }
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class FeatureFlagAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _featureName;
    private readonly bool _returnNotFound;

    /// <summary>
    /// Creates a feature flag gate.
    /// </summary>
    /// <param name="featureName">Name of the feature flag to check</param>
    /// <param name="returnNotFound">If true, returns 404; otherwise returns 403</param>
    public FeatureFlagAttribute(string featureName, bool returnNotFound = true)
    {
        _featureName = featureName;
        _returnNotFound = returnNotFound;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureFlagService = context.HttpContext.RequestServices.GetService<IFeatureFlagService>();

        if (featureFlagService == null)
        {
            // If service not registered, allow access (fail open)
            await next();
            return;
        }

        var isEnabled = await featureFlagService.IsEnabledAsync(_featureName);

        if (!isEnabled)
        {
            if (_returnNotFound)
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                context.Result = new StatusCodeResult(403);
            }
            return;
        }

        await next();
    }
}

/// <summary>
/// Attribute for feature flags with context-aware evaluation.
/// Checks user/tenant specific feature access.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class FeatureFlagWithContextAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _featureName;

    public FeatureFlagWithContextAttribute(string featureName)
    {
        _featureName = featureName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureFlagService = context.HttpContext.RequestServices.GetService<IFeatureFlagService>();

        if (featureFlagService == null)
        {
            await next();
            return;
        }

        // Build context from current user
        var featureContext = new FeatureFlagContext
        {
            UserId = context.HttpContext.User.FindFirst("sub")?.Value ??
                     context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Role = context.HttpContext.User.FindFirst("role")?.Value ??
                   context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        };

        var ssIdClaim = context.HttpContext.User.FindFirst("ssid")?.Value ??
                        context.HttpContext.User.FindFirst("SSId")?.Value;
        if (int.TryParse(ssIdClaim, out var tenantId))
        {
            featureContext.TenantId = tenantId;
        }

        var isEnabled = featureFlagService.IsEnabled(_featureName, featureContext);

        if (!isEnabled)
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}

