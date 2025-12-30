using System.Diagnostics;
using BuildingBlocks.API.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BuildingBlocks.API.Filters;

/// <summary>
/// Action filter that wraps successful responses in ApiResponse format.
/// Automatically applied to all controllers when registered.
/// </summary>
public class ApiResponseWrapperFilter : IAsyncResultFilter
{
    /// <inheritdoc/>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        if (context.Result is ObjectResult objectResult)
        {
            // Skip if already wrapped or is ProblemDetails
            if (objectResult.Value is ApiResponse<object> || 
                objectResult.Value is ApiProblemDetails ||
                objectResult.Value is ProblemDetails)
            {
                await next();
                return;
            }

            // Skip for error status codes (let exception middleware handle)
            if (objectResult.StatusCode >= 400)
            {
                await next();
                return;
            }

            // Wrap successful responses
            var wrappedResponse = new ApiResponse<object>
            {
                Success = true,
                Data = objectResult.Value,
                TraceId = traceId
            };

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = objectResult.StatusCode ?? 200
            };
        }
        else if (context.Result is EmptyResult || context.Result is NoContentResult)
        {
            // Wrap empty responses
            var wrappedResponse = new ApiResponse<object>
            {
                Success = true,
                Message = "Operation completed successfully.",
                TraceId = traceId
            };

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = 200
            };
        }
        else if (context.Result is CreatedAtActionResult createdResult)
        {
            // Wrap CreatedAtAction responses
            var wrappedResponse = new ApiResponse<object>
            {
                Success = true,
                Data = createdResult.Value,
                Message = "Resource created successfully.",
                TraceId = traceId
            };

            context.Result = new CreatedAtActionResult(
                createdResult.ActionName,
                createdResult.ControllerName,
                createdResult.RouteValues,
                wrappedResponse)
            {
                StatusCode = 201
            };
        }

        await next();
    }
}

/// <summary>
/// Attribute to opt-out of response wrapping for specific actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SkipApiResponseWrapperAttribute : Attribute
{
}

