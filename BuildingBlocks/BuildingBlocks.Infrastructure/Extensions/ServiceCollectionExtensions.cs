using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Infrastructure.FeatureFlags;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering BuildingBlocks.Infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds generic repository pattern implementation to the service collection.
    /// </summary>
    /// <typeparam name="TContext">DbContext type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRepositories<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        // Register open generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));

        return services;
    }

    /// <summary>
    /// Adds the auditable entity interceptor for automatic audit field population.
    /// Call this BEFORE AddDbContext to ensure interceptor is registered.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAuditableEntityInterceptor(this IServiceCollection services)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        return services;
    }

    /// <summary>
    /// Adds feature flag service for runtime feature toggles.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        return services;
    }

    /// <summary>
    /// Adds all BuildingBlocks.Infrastructure services including:
    /// - Generic repositories
    /// - Auditable entity interceptor
    /// - System clock
    /// - Feature flags
    /// </summary>
    /// <typeparam name="TContext">DbContext type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBuildingBlocksInfrastructure<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddAuditableEntityInterceptor();
        services.AddRepositories<TContext>();
        services.AddFeatureFlags();

        return services;
    }
}

