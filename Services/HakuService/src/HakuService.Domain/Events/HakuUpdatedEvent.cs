using BuildingBlocks.Domain;

namespace HakuService.Domain.Events;

/// <summary>
/// Domain event raised when a Haku is updated
/// </summary>
public record HakuUpdatedEvent(int HakuId, string Name, int SSId) : DomainEvent;

