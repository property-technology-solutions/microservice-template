using BuildingBlocks.Application;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Security;
using HakuService.Application.Common.Interfaces;
using HakuService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HakuService.Infrastructure.Persistence;

/// <summary>
/// Application database context with multi-tenancy and soft delete support.
/// Implements IUnitOfWork for transaction management.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly IClock _clock;
    private readonly ICurrentUserService? _currentUserService;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _currentTransaction;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IClock clock,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _clock = clock;
        _currentUserService = currentUserService;
    }

    public DbSet<Haku> Hakus => Set<Haku>();
    public DbSet<HakuTranslation> HakuTranslations { get; set; } = null!;

    /// <summary>
    /// Current tenant ID for multi-tenancy filtering.
    /// Null means no tenant filtering (admin access).
    /// </summary>
    public int? TenantId => _currentUserService?.SSId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply global query filter for multi-tenancy
        // Note: Soft delete filter is applied in entity configurations
        ApplyMultiTenancyFilter(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Applies multi-tenancy filter to all entities with SSId property.
    /// Entities where SSId is null are considered global (accessible to all tenants).
    /// </summary>
    private void ApplyMultiTenancyFilter(ModelBuilder modelBuilder)
    {
        // Haku entity - filter by tenant
        modelBuilder.Entity<Haku>().HasQueryFilter(e => 
            e.Status == 1 && // Soft delete
            (TenantId == null || e.SSId == null || e.SSId == TenantId)); // Multi-tenancy

        // HakuTranslation entity - filter by tenant
        modelBuilder.Entity<HakuTranslation>().HasQueryFilter(e =>
            e.Status == 1 && // Soft delete
            (TenantId == null || e.SSId == null || e.SSId == TenantId)); // Multi-tenancy
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No active transaction");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}

