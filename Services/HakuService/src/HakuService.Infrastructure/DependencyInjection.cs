using BuildingBlocks.Application;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Security;
using HakuService.Application.Common.Interfaces;
using HakuService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HakuService.Infrastructure;

/// <summary>
/// Dependency injection registration for HakuService.Infrastructure.
/// Provides a clean, centralized way to register all infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds HakuService infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddHakuServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add BuildingBlocks.Infrastructure services (Clock, Audit Interceptor, Repositories)
        services.AddBuildingBlocksInfrastructure<ApplicationDbContext>();

        // Database context with interceptors
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
            })
            .UseSnakeCaseNamingConvention();

            // Add auditable entity interceptor
            var auditInterceptor = sp.GetService<AuditableEntityInterceptor>();
            if (auditInterceptor != null)
            {
                options.AddInterceptors(auditInterceptor);
            }
        });

        // Register interfaces
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}

