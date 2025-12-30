using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.API.Responses;

/// <summary>
/// RFC 7807 compliant problem details for API error responses.
/// Extends the standard ProblemDetails with additional enterprise fields.
/// </summary>
public class ApiProblemDetails : ProblemDetails
{
    /// <summary>
    /// Distributed tracing identifier for debugging.
    /// </summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    /// <summary>
    /// Detailed validation errors keyed by field name.
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// UTC timestamp when the error occurred.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a validation error (400 Bad Request).
    /// </summary>
    public static ApiProblemDetails ValidationError(
        IDictionary<string, string[]> errors,
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Validation Error",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Instance = instance,
            TraceId = traceId,
            Errors = errors
        };
    }

    /// <summary>
    /// Creates a validation error from a list of error messages.
    /// </summary>
    public static ApiProblemDetails ValidationError(
        IEnumerable<string> errors,
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Validation Error",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Instance = instance,
            TraceId = traceId,
            Errors = new Dictionary<string, string[]>
            {
                { "errors", errors.ToArray() }
            }
        };
    }

    /// <summary>
    /// Creates a not found error (404).
    /// </summary>
    public static ApiProblemDetails NotFound(
        string detail,
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Not Found",
            Status = 404,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates an unauthorized error (401).
    /// </summary>
    public static ApiProblemDetails Unauthorized(
        string detail = "Authentication is required to access this resource.",
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Unauthorized",
            Status = 401,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates a forbidden error (403).
    /// </summary>
    public static ApiProblemDetails Forbidden(
        string detail = "You do not have permission to access this resource.",
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Forbidden",
            Status = 403,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates a conflict error (409).
    /// </summary>
    public static ApiProblemDetails Conflict(
        string detail,
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Conflict",
            Status = 409,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates an internal server error (500).
    /// </summary>
    public static ApiProblemDetails InternalServerError(
        string detail = "An unexpected error occurred. Please try again later.",
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Internal Server Error",
            Status = 500,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates a service unavailable error (503).
    /// </summary>
    public static ApiProblemDetails ServiceUnavailable(
        string detail = "The service is temporarily unavailable. Please try again later.",
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Service Unavailable",
            Status = 503,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Creates a bad request error (400) for business logic errors.
    /// </summary>
    public static ApiProblemDetails BadRequest(
        string detail,
        string? instance = null,
        string? traceId = null)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Bad Request",
            Status = 400,
            Detail = detail,
            Instance = instance,
            TraceId = traceId
        };
    }
}

