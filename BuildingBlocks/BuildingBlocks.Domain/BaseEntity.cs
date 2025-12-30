namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for all entities in the system
/// Provides common properties like Id, audit fields, and multi-tenancy support
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Shopping Center ID for multi-tenancy isolation
    /// Null for global entities
    /// </summary>
    public int? SSId { get; set; }

    /// <summary>
    /// UTC timestamp when entity was created
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when entity was last updated
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// User ID who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Entity status: 1 = Active, 0 = Inactive/Deleted
    /// Use for soft delete functionality
    /// </summary>
    public int? Status { get; set; } = 1;

    /// <summary>
    /// Optimistic concurrency control
    /// EF Core automatically updates this on each save
    /// </summary>
    public byte[]? RowVersion { get; set; }
}

