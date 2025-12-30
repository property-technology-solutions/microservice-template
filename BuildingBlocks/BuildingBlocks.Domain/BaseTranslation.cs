namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for translation entities
/// Provides common properties for multi-language support
/// </summary>
public abstract class BaseTranslation : BaseEntity, ITranslation
{
    /// <summary>
    /// Language code (ISO 639-1)
    /// Examples: "en", "tr", "ar", "de", "fr"
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;
}

