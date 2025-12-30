using BuildingBlocks.API.Filters;
using BuildingBlocks.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.API.Extensions;

/// <summary>
/// Extension methods for registering BuildingBlocks.API services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds standard API response formatting including:
    /// - API response wrapper filter
    /// - Problem details configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApiResponseFormatting(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiResponseWrapperFilter>();
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            // Disable default model state validation response
            // We handle it in ValidationBehavior + GlobalExceptionMiddleware
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

    /// <summary>
    /// Adds all BuildingBlocks.API services including:
    /// - API response formatting
    /// - Standard configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddBuildingBlocksApi(this IServiceCollection services)
    {
        services.AddApiResponseFormatting();
        return services;
    }
}

/// <summary>
/// Extension methods for configuring BuildingBlocks.API middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds input sanitization middleware to the pipeline.
    /// Should be added early in the pipeline before request processing.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="options">Optional sanitization options</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseInputSanitization(
        this IApplicationBuilder app,
        InputSanitizationOptions? options = null)
    {
        return app.UseMiddleware<InputSanitizationMiddleware>(options ?? new InputSanitizationOptions());
    }

    /// <summary>
    /// Adds all BuildingBlocks.API middleware in the correct order.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseBuildingBlocksApi(this IApplicationBuilder app)
    {
        app.UseInputSanitization();
        return app;
    }
}

