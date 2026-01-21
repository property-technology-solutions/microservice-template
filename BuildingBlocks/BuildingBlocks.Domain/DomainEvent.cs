using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for all domain events
/// Domain events represent something that happened in the domain
/// They are handled within the same bounded context
/// Note: NotMapped to prevent EF Core from treating it as an entity
/// </summary>
[NotMapped]
public abstract record DomainEvent : INotification
{
    /// <summary>
    /// UTC timestamp when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier for the event
    /// Useful for idempotency and event tracking
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();
}

