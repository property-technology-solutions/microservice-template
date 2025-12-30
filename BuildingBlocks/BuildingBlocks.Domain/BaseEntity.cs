namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for all entities in the system.
/// Provides common properties - all optional except Id.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity (UUID/GUID)
    /// Auto-generated if not provided
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Shopping Center ID for multi-tenancy isolation.
    /// Null for global entities or when not using multi-tenancy.
    /// </summary>
    public int? SSId { get; set; }

    /// <summary>
    /// UTC timestamp when entity was created.
    /// Null if not tracking creation time.
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// User ID who created the entity.
    /// Null if not tracking creator.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when entity was last updated.
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// User ID who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Entity status for soft delete: 1 = Active, 0 = Deleted.
    /// Null if not using soft delete.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Optimistic concurrency control.
    /// EF Core automatically updates this on each save.
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
