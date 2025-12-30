namespace BuildingBlocks.Domain;

/// <summary>
/// Marker interface for entities that support translations
/// </summary>
public interface ITranslatable
{
    /// <summary>
    /// Collection of translations for this entity
    /// </summary>
    IReadOnlyCollection<ITranslation> Translations { get; }
}

/// <summary>
/// Interface for translation entities
/// </summary>
public interface ITranslation
{
    /// <summary>
    /// Language code (ISO 639-1): en, tr, ar, de, etc.
    /// </summary>
    string LanguageCode { get; }
}

