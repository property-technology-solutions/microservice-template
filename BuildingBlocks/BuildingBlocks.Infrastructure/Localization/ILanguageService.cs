namespace BuildingBlocks.Infrastructure.Localization;

/// <summary>
/// Service for managing current language context
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Get current language code
    /// Returns language from Accept-Language header or default
    /// </summary>
    string GetCurrentLanguage();

    /// <summary>
    /// Get default language code
    /// </summary>
    string GetDefaultLanguage();

    /// <summary>
    /// Check if language code is supported
    /// </summary>
    bool IsLanguageSupported(string? languageCode);

    /// <summary>
    /// Get all supported languages
    /// </summary>
    IReadOnlyList<string> GetSupportedLanguages();
}

