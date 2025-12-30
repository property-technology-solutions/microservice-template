using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace BuildingBlocks.API.RateLimiting;

/// <summary>
/// Simple rate limiting middleware for .NET 9
/// Limits requests per IP address using sliding window algorithm
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _requestLimit;
    private readonly TimeSpan _timeWindow;
    private static readonly ConcurrentDictionary<string, RequestCounter> _clients = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        int requestLimit = 100,
        int timeWindowSeconds = 60)
    {
        _next = next;
        _requestLimit = requestLimit;
        _timeWindow = TimeSpan.FromSeconds(timeWindowSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var counter = _clients.GetOrAdd(clientId, _ => new RequestCounter());

        bool isLimitExceeded = false;
        
        // Check limit without holding lock during async operation
        lock (counter.Lock)
        {
            // Clean old requests
            counter.Requests.RemoveAll(r => r < DateTime.UtcNow - _timeWindow);

            // Check limit
            if (counter.Requests.Count >= _requestLimit)
            {
                isLimitExceeded = true;
            }
            else
            {
                // Add current request
                counter.Requests.Add(DateTime.UtcNow);
            }
        }

        if (isLimitExceeded)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID first (if authenticated)
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private class RequestCounter
    {
        public object Lock { get; } = new object();
        public List<DateTime> Requests { get; } = new();
    }
}

/// <summary>
/// Extension methods for adding rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Add rate limiting middleware to the pipeline
    /// Default: 100 requests per 60 seconds per client
    /// </summary>
    public static IApplicationBuilder UseSimpleRateLimiting(
        this IApplicationBuilder app,
        int requestLimit = 100,
        int timeWindowSeconds = 60)
    {
        return app.UseMiddleware<RateLimitingMiddleware>(requestLimit, timeWindowSeconds);
    }
}

