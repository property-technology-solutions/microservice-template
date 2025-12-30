using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace BuildingBlocks.Infrastructure.Resilience;

/// <summary>
/// Pre-configured Polly resilience policies for HTTP clients
/// Provides retry and circuit breaker patterns
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Retry policy with exponential backoff
    /// Retries 3 times with delays: 1s, 2s, 4s
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger? logger = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to {Reason}",
                        retryCount, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                });
    }

    /// <summary>
    /// Circuit breaker policy
    /// Opens circuit after 5 consecutive failures
    /// Stays open for 30 seconds before attempting to close
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger? logger = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    logger?.LogError(
                        "Circuit breaker opened for {Duration}s due to {Reason}",
                        duration.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                },
                onReset: () =>
                {
                    logger?.LogInformation("Circuit breaker reset");
                });
    }
}

