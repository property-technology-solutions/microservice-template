using BuildingBlocks.Domain;

namespace HakuService.Domain.Entities;

/// <summary>
/// Translation entity for Haku multi-language support
/// </summary>
public class HakuTranslation : BaseTranslation
{
    /// <summary>
    /// Foreign key to Haku
    /// </summary>
    public int HakuId { get; set; }

    /// <summary>
    /// Translated name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property
    /// </summary>
    public Haku? Haku { get; set; }
}
