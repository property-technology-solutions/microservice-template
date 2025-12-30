using BuildingBlocks.Domain;

namespace HakuService.Domain.Events;

/// <summary>
/// Domain event raised when a Haku is deleted (soft delete)
/// </summary>
public record HakuDeletedEvent(int HakuId, string Name) : DomainEvent;

