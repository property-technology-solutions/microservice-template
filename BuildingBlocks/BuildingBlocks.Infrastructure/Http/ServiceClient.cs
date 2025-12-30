using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Implementation of service client with resilience
/// Uses HttpClientFactory with Polly policies
/// </summary>
public class ServiceClient : IServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServiceClient> _logger;

    public ServiceClient(IHttpClientFactory httpClientFactory, ILogger<ServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<TResponse?> GetAsync<TResponse>(
        string serviceUrl,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ResilientHttpClient");
        var url = $"{serviceUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        _logger.LogInformation("Sending GET request to {Url}", url);

        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            
            _logger.LogInformation("GET request successful: {Url}", url);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request failed: {Url}", url);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string serviceUrl,
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ResilientHttpClient");
        var url = $"{serviceUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        _logger.LogInformation("Sending POST request to {Url}", url);

        try
        {
            var response = await client.PostAsJsonAsync(url, data, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            
            _logger.LogInformation("POST request successful: {Url}", url);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request failed: {Url}", url);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string serviceUrl,
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ResilientHttpClient");
        var url = $"{serviceUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        _logger.LogInformation("Sending PUT request to {Url}", url);

        try
        {
            var response = await client.PutAsJsonAsync(url, data, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            
            _logger.LogInformation("PUT request successful: {Url}", url);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT request failed: {Url}", url);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        string serviceUrl,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ResilientHttpClient");
        var url = $"{serviceUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        _logger.LogInformation("Sending DELETE request to {Url}", url);

        try
        {
            var response = await client.DeleteAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("DELETE request successful: {Url}", url);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE request failed: {Url}", url);
            return false;
        }
    }
}

