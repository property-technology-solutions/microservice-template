namespace BuildingBlocks.Infrastructure.Cache;

/// <summary>
/// Distributed cache abstraction
/// Implementation uses Redis
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// Returns null if key doesn't exist or has expired
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or set pattern: Get from cache, if not found execute factory and cache result
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;
}

