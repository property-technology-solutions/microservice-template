using BuildingBlocks.Domain;
using HakuService.Domain.Events;

namespace HakuService.Domain.Entities;

/// <summary>
/// Haku aggregate root - Example entity for template
/// Minimal, clean implementation following DDD principles
/// </summary>
public class Haku : BaseEntity, IAggregateRoot, ITranslatable
{
    private readonly List<DomainEvent> _domainEvents = new();
    private readonly List<HakuTranslation> _translations = new();

    /// <summary>
    /// Haku name (default language)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this Haku is featured
    /// </summary>
    public bool IsFeatured { get; private set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// Translations for multi-language support
    /// </summary>
    public IReadOnlyCollection<HakuTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Domain events raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    IReadOnlyCollection<ITranslation> ITranslatable.Translations => 
        _translations.Cast<ITranslation>().ToList().AsReadOnly();

    // Private constructor for EF Core
    private Haku() { }

    /// <summary>
    /// Factory method to create a new Haku
    /// </summary>
    public static Haku Create(string name, int ssId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Haku name cannot be empty", nameof(name));

        if (ssId <= 0)
            throw new ArgumentException("SSId must be greater than zero", nameof(ssId));

        var haku = new Haku
        {
            Name = name,
            SSId = ssId,
            Status = 1,
            Created = DateTime.UtcNow
        };

        haku._domainEvents.Add(new HakuCreatedEvent(haku.Id, haku.Name, haku.SSId!.Value));

        return haku;
    }

    /// <summary>
    /// Update Haku information
    /// </summary>
    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Haku name cannot be empty", nameof(name));

        if (Name != name)
        {
            Name = name;
            Updated = DateTime.UtcNow;
            _domainEvents.Add(new HakuUpdatedEvent(Id, Name, SSId!.Value));
        }
    }

    /// <summary>
    /// Mark Haku as featured
    /// </summary>
    public void MarkAsFeatured()
    {
        if (!IsFeatured)
        {
            IsFeatured = true;
            Updated = DateTime.UtcNow;
            _domainEvents.Add(new HakuFeaturedEvent(Id, Name));
        }
    }

    /// <summary>
    /// Remove featured status
    /// </summary>
    public void RemoveFeaturedStatus()
    {
        if (IsFeatured)
        {
            IsFeatured = false;
            Updated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Soft delete
    /// </summary>
    public void Delete()
    {
        if (Status == 1)
        {
            Status = 0;
            Updated = DateTime.UtcNow;
            _domainEvents.Add(new HakuDeletedEvent(Id, Name));
        }
    }

    /// <summary>
    /// Add or update translation
    /// </summary>
    public void SetTranslation(string languageCode, string name)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new ArgumentException("Language code is required", nameof(languageCode));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Translated name is required", nameof(name));

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        if (existing != null)
        {
            existing.Name = name;
        }
        else
        {
            _translations.Add(new HakuTranslation
            {
                HakuId = Id,
                LanguageCode = languageCode,
                Name = name,
                SSId = SSId,
                Status = 1,
                Created = DateTime.UtcNow
            });
        }

        Updated = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove translation
    /// </summary>
    public void RemoveTranslation(string languageCode)
    {
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation != null)
        {
            _translations.Remove(translation);
            Updated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clear domain events after dispatch
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
