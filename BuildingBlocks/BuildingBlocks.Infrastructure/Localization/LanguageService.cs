using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.Localization;

/// <summary>
/// Implementation of language service
/// Extracts language from HTTP Accept-Language header
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _defaultLanguage;
    private readonly List<string> _supportedLanguages;

    public LanguageService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _defaultLanguage = configuration["Localization:DefaultLanguage"] ?? "tr";
        
        var supportedLangs = configuration["Localization:SupportedLanguages"] ?? "tr,en";
        _supportedLanguages = supportedLangs.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().ToLowerInvariant())
            .ToList();
    }

    public string GetCurrentLanguage()
    {
        // Try to get from Accept-Language header
        var acceptLanguage = _httpContextAccessor.HttpContext?
            .Request.Headers["Accept-Language"].ToString();

        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return _defaultLanguage;

        // Parse Accept-Language header (e.g., "en-US,en;q=0.9,tr;q=0.8")
        var languages = acceptLanguage
            .Split(',')
            .Select(lang => lang.Split(';')[0].Trim().ToLowerInvariant())
            .Select(lang => lang.Length > 2 ? lang.Substring(0, 2) : lang);

        // Find first supported language
        foreach (var lang in languages)
        {
            if (_supportedLanguages.Contains(lang))
                return lang;
        }

        return _defaultLanguage;
    }

    public string GetDefaultLanguage() => _defaultLanguage;

    public bool IsLanguageSupported(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return false;
        
        return _supportedLanguages.Contains(languageCode.ToLowerInvariant());
    }

    public IReadOnlyList<string> GetSupportedLanguages() => _supportedLanguages.AsReadOnly();
}

