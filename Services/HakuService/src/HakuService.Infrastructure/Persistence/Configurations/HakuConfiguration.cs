using HakuService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HakuService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Haku entity
/// </summary>
public class HakuConfiguration : IEntityTypeConfiguration<Haku>
{
    public void Configure(EntityTypeBuilder<Haku> builder)
    {
        builder.ToTable("hakus");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(h => h.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.OrderNo)
            .HasColumnName("order_no")
            .HasDefaultValue(0);

        builder.Property(h => h.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        builder.Property(h => h.SSId)
            .HasColumnName("ss_id");

        builder.Property(h => h.Created)
            .HasColumnName("created")
            .IsRequired();

        builder.Property(h => h.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(h => h.Updated)
            .HasColumnName("updated");

        builder.Property(h => h.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(h => h.Status)
            .HasColumnName("status")
            .HasDefaultValue(1);

        builder.Property(h => h.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(h => h.SSId)
            .HasDatabaseName("idx_hakus_ss_id");

        builder.HasIndex(h => h.Name)
            .HasDatabaseName("idx_hakus_name");

        builder.HasIndex(h => h.Status)
            .HasDatabaseName("idx_hakus_status");

        builder.HasIndex(h => h.IsFeatured)
            .HasDatabaseName("idx_hakus_is_featured");

        // Note: Global query filter for soft delete and multi-tenancy
        // is applied in ApplicationDbContext.ApplyMultiTenancyFilter()

        // Ignore domain events (not mapped to database)
        builder.Ignore(h => h.DomainEvents);
    }
}
