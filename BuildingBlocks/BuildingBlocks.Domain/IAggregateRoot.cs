namespace BuildingBlocks.Domain;

/// <summary>
/// Marker interface for aggregate roots in Domain-Driven Design
/// Only aggregate roots should be directly accessed by repositories
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Collection of domain events raised by this aggregate
    /// Events are dispatched after successful database transaction
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clear all domain events
    /// Called after events have been dispatched
    /// </summary>
    void ClearDomainEvents();
}

