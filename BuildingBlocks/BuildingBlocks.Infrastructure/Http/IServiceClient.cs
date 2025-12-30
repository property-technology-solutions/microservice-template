namespace BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Generic HTTP client for service-to-service communication
/// Includes resilience patterns (circuit breaker, retry)
/// </summary>
public interface IServiceClient
{
    /// <summary>
    /// Send GET request to another microservice
    /// </summary>
    Task<TResponse?> GetAsync<TResponse>(string serviceUrl, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send POST request to another microservice
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string serviceUrl, string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send PUT request to another microservice
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string serviceUrl, string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send DELETE request to another microservice
    /// </summary>
    Task<bool> DeleteAsync(string serviceUrl, string endpoint, CancellationToken cancellationToken = default);
}

