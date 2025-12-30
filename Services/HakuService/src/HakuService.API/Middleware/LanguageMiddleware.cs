using BuildingBlocks.Infrastructure.Localization;

namespace HakuService.API.Middleware;

/// <summary>
/// Middleware to set current language from Accept-Language header
/// </summary>
public class LanguageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LanguageMiddleware> _logger;

    public LanguageMiddleware(RequestDelegate next, ILogger<LanguageMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILanguageService languageService)
    {
        var language = languageService.GetCurrentLanguage();
        
        _logger.LogDebug("Request language set to: {Language}", language);
        
        // Add language to response headers for debugging
        context.Response.Headers.Append("Content-Language", language);
        
        await _next(context);
    }
}

