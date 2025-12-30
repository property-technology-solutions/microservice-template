using BuildingBlocks.Domain;
using BuildingBlocks.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// EF Core interceptor that automatically sets audit fields (CreatedBy, UpdatedBy, Created, Updated)
/// on entities before they are saved to the database.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService, IClock clock)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        var utcNow = _clock.UtcNow;
        var userId = _currentUserService.UserId;
        var ssId = _currentUserService.SSId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Created = utcNow;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.Status ??= 1; // Default to active

                    // Set SSId if not already set and user has SSId
                    if (entry.Entity.SSId is null && ssId.HasValue)
                    {
                        entry.Entity.SSId = ssId;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.Updated = utcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }
}

