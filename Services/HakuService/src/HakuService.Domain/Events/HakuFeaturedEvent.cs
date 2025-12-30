using BuildingBlocks.Domain;

namespace HakuService.Domain.Events;

/// <summary>
/// Domain event raised when a Haku is marked as featured
/// </summary>
public record HakuFeaturedEvent(int HakuId, string Name) : DomainEvent;

