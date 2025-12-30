using HakuService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HakuService.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// Allows application layer to access database without depending on infrastructure
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Haku> Hakus { get; }
    DbSet<HakuTranslation> HakuTranslations { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

