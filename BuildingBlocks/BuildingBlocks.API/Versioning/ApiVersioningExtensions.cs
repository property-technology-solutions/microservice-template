using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.API.Versioning;

/// <summary>
/// Extension methods for API versioning setup
/// Supports URL-based and header-based versioning
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Add API versioning with default configuration
    /// URL-based: /api/v1/hakus, /api/v2/hakus
    /// Header-based: api-version: 1.0
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default version if not specified
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true; // Add api-supported-versions header

            // Support both URL and header versioning
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("api-version"),
                new QueryStringApiVersionReader("api-version")
            );
        })
        .AddApiExplorer(options =>
        {
            // Format: 'v'major[.minor][-status]
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}

