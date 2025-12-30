using System.Text.Json.Serialization;

namespace BuildingBlocks.API.Responses;

/// <summary>
/// Standard API response wrapper for successful operations.
/// Provides consistent response format across all API endpoints.
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Response data payload.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    /// <summary>
    /// Optional message providing additional context.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    /// <summary>
    /// UTC timestamp of the response.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Distributed tracing identifier for debugging.
    /// </summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates a successful response without data.
    /// </summary>
    public static ApiResponse<T> Ok(string? message = null, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            TraceId = traceId
        };
    }
}

/// <summary>
/// Non-generic API response for operations that don't return data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response without data.
    /// </summary>
    public new static ApiResponse Ok(string? message = null, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            TraceId = traceId
        };
    }
}

