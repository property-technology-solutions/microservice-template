using BuildingBlocks.Domain;

namespace HakuService.Domain.Events;

/// <summary>
/// Domain event raised when a new Haku is created
/// </summary>
public record HakuCreatedEvent(Guid HakuId, string Name, int SSId) : DomainEvent;

