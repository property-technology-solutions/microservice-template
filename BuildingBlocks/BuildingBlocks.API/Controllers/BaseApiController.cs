using System.Diagnostics;
using System.Security.Claims;
using BuildingBlocks.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.API.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// Includes standard result methods and access to current user information.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the current trace ID for distributed tracing.
    /// </summary>
    protected string TraceId => Activity.Current?.Id ?? HttpContext.TraceIdentifier;

    /// <summary>
    /// Gets the current user's ID from JWT claims.
    /// </summary>
    protected string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                       User.FindFirst("sub")?.Value;

    /// <summary>
    /// Gets the current user's email from JWT claims.
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value ??
                                          User.FindFirst("email")?.Value;

    /// <summary>
    /// Gets the current user's role from JWT claims.
    /// </summary>
    protected string? CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value ??
                                         User.FindFirst("role")?.Value;

    /// <summary>
    /// Gets the Shopping Center ID (SSId) for multi-tenancy from JWT claims.
    /// </summary>
    protected int? CurrentSSId
    {
        get
        {
            var ssIdClaim = User.FindFirst("ssid")?.Value ??
                           User.FindFirst("SSId")?.Value;
            return int.TryParse(ssIdClaim, out var ssId) ? ssId : null;
        }
    }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    protected IActionResult ApiOk<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message, TraceId));
    }

    /// <summary>
    /// Creates a successful response without data.
    /// </summary>
    protected IActionResult ApiOk(string? message = null)
    {
        return Ok(ApiResponse.Ok(message, TraceId));
    }

    /// <summary>
    /// Creates a created response (201) with data.
    /// </summary>
    protected IActionResult ApiCreated<T>(T data, string? actionName = null, object? routeValues = null, string? message = null)
    {
        var response = ApiResponse<T>.Ok(data, message ?? "Resource created successfully.", TraceId);
        
        if (actionName != null)
        {
            return CreatedAtAction(actionName, routeValues, response);
        }

        return StatusCode(201, response);
    }

    /// <summary>
    /// Creates a not found response (404).
    /// </summary>
    protected IActionResult ApiNotFound(string detail)
    {
        var problemDetails = ApiProblemDetails.NotFound(detail, Request.Path, TraceId);
        return NotFound(problemDetails);
    }

    /// <summary>
    /// Creates a bad request response (400) with validation errors.
    /// </summary>
    protected IActionResult ApiBadRequest(IDictionary<string, string[]> errors)
    {
        var problemDetails = ApiProblemDetails.ValidationError(errors, Request.Path, TraceId);
        return BadRequest(problemDetails);
    }

    /// <summary>
    /// Creates a bad request response (400) with error messages.
    /// </summary>
    protected IActionResult ApiBadRequest(IEnumerable<string> errors)
    {
        var problemDetails = ApiProblemDetails.ValidationError(errors, Request.Path, TraceId);
        return BadRequest(problemDetails);
    }

    /// <summary>
    /// Creates a bad request response (400) with a single error message.
    /// </summary>
    protected IActionResult ApiBadRequest(string error)
    {
        var problemDetails = ApiProblemDetails.BadRequest(error, Request.Path, TraceId);
        return BadRequest(problemDetails);
    }

    /// <summary>
    /// Creates an unauthorized response (401).
    /// </summary>
    protected IActionResult ApiUnauthorized(string? detail = null)
    {
        var problemDetails = ApiProblemDetails.Unauthorized(
            detail ?? "Authentication is required to access this resource.",
            Request.Path,
            TraceId);
        return Unauthorized(problemDetails);
    }

    /// <summary>
    /// Creates a forbidden response (403).
    /// </summary>
    protected IActionResult ApiForbidden(string? detail = null)
    {
        var problemDetails = ApiProblemDetails.Forbidden(
            detail ?? "You do not have permission to access this resource.",
            Request.Path,
            TraceId);
        return StatusCode(403, problemDetails);
    }

    /// <summary>
    /// Creates a conflict response (409).
    /// </summary>
    protected IActionResult ApiConflict(string detail)
    {
        var problemDetails = ApiProblemDetails.Conflict(detail, Request.Path, TraceId);
        return Conflict(problemDetails);
    }

    /// <summary>
    /// Creates an internal server error response (500).
    /// </summary>
    protected IActionResult ApiServerError(string? detail = null)
    {
        var problemDetails = ApiProblemDetails.InternalServerError(
            detail ?? "An unexpected error occurred.",
            Request.Path,
            TraceId);
        return StatusCode(500, problemDetails);
    }
}

