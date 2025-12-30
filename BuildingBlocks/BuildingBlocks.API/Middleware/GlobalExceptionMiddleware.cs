using System.Diagnostics;
using System.Text.Json;
using BuildingBlocks.API.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions and returns RFC 7807 compliant error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var path = context.Request.Path;

        var problemDetails = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx, path, traceId),
            ArgumentException argEx => HandleArgumentException(argEx, path, traceId),
            UnauthorizedAccessException => HandleUnauthorizedException(path, traceId),
            KeyNotFoundException notFoundEx => HandleNotFoundException(notFoundEx, path, traceId),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx, path, traceId),
            OperationCanceledException => HandleOperationCancelledException(path, traceId),
            _ => HandleGenericException(exception, path, traceId)
        };

        // Log the exception
        LogException(exception, problemDetails);

        // Write response
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? 500;

        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private ApiProblemDetails HandleValidationException(ValidationException ex, string path, string traceId)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return ApiProblemDetails.ValidationError(errors, path, traceId);
    }

    private ApiProblemDetails HandleArgumentException(ArgumentException ex, string path, string traceId)
    {
        return ApiProblemDetails.BadRequest(ex.Message, path, traceId);
    }

    private ApiProblemDetails HandleUnauthorizedException(string path, string traceId)
    {
        return ApiProblemDetails.Unauthorized(instance: path, traceId: traceId);
    }

    private ApiProblemDetails HandleNotFoundException(KeyNotFoundException ex, string path, string traceId)
    {
        return ApiProblemDetails.NotFound(ex.Message, path, traceId);
    }

    private ApiProblemDetails HandleInvalidOperationException(InvalidOperationException ex, string path, string traceId)
    {
        return ApiProblemDetails.BadRequest(ex.Message, path, traceId);
    }

    private ApiProblemDetails HandleOperationCancelledException(string path, string traceId)
    {
        return new ApiProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
            Title = "Request Cancelled",
            Status = 499, // Client Closed Request
            Detail = "The request was cancelled.",
            Instance = path,
            TraceId = traceId
        };
    }

    private ApiProblemDetails HandleGenericException(Exception ex, string path, string traceId)
    {
        var detail = _environment.IsDevelopment()
            ? ex.Message
            : "An unexpected error occurred. Please try again later.";

        var problemDetails = ApiProblemDetails.InternalServerError(detail, path, traceId);

        // Include stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = ex.StackTrace;
            problemDetails.Extensions["exceptionType"] = ex.GetType().Name;
        }

        return problemDetails;
    }

    private void LogException(Exception exception, ApiProblemDetails problemDetails)
    {
        var statusCode = problemDetails.Status ?? 500;

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Status: {Status}",
                problemDetails.TraceId,
                problemDetails.Instance,
                statusCode);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "Client error. TraceId: {TraceId}, Path: {Path}, Status: {Status}, Detail: {Detail}",
                problemDetails.TraceId,
                problemDetails.Instance,
                statusCode,
                problemDetails.Detail);
        }
    }
}

