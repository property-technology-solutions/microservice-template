using System.Diagnostics;

namespace HakuService.API.Middleware;

/// <summary>
/// Middleware to handle correlation IDs for distributed tracing
/// Generates or forwards X-Correlation-ID header across service calls
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get correlation ID from header or generate new one
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Add to response headers
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        // Add to HttpContext items for use in application
        context.Items["CorrelationId"] = correlationId;

        // Add to OpenTelemetry activity
        Activity.Current?.SetTag("correlation_id", correlationId);

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            _logger.LogDebug("Processing request with correlation ID: {CorrelationId}", correlationId);
            await _next(context);
        }
    }
}

