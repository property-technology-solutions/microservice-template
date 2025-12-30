using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Extensions;

/// <summary>
/// Extension methods for querying translatable entities
/// Automatically includes translations and applies fallback logic
/// </summary>
public static class TranslationExtensions
{
    /// <summary>
    /// Include translations for an entity
    /// Use this in queries where you need translated content
    /// </summary>
    public static IQueryable<T> IncludeTranslations<T>(this IQueryable<T> query) 
        where T : class, ITranslatable
    {
        return query.Include(e => e.Translations);
    }

    /// <summary>
    /// Get translated value with fallback
    /// Returns translated value if exists for given language, otherwise returns default value
    /// </summary>
    public static string GetTranslated(
        this ITranslatable entity,
        string languageCode,
        Func<ITranslation, string?> translationSelector,
        string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return defaultValue;

        var translation = entity.Translations
            .FirstOrDefault(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

        if (translation == null)
            return defaultValue;

        var translatedValue = translationSelector(translation);
        return string.IsNullOrWhiteSpace(translatedValue) ? defaultValue : translatedValue;
    }
}

