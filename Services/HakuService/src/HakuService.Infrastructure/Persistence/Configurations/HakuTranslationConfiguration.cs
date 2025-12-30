using HakuService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HakuService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for HakuTranslation entity
/// </summary>
public class HakuTranslationConfiguration : IEntityTypeConfiguration<HakuTranslation>
{
    public void Configure(EntityTypeBuilder<HakuTranslation> builder)
    {
        builder.ToTable("haku_translations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.HakuId)
            .HasColumnName("haku_id")
            .IsRequired();

        builder.Property(t => t.LanguageCode)
            .HasColumnName("language_code")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.SSId)
            .HasColumnName("ss_id");

        builder.Property(t => t.Created)
            .HasColumnName("created")
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(t => t.Updated)
            .HasColumnName("updated");

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasDefaultValue(1);

        builder.Property(t => t.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Relationship
        builder.HasOne(t => t.Haku)
            .WithMany()
            .HasForeignKey(t => t.HakuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => new { t.HakuId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("idx_haku_translations_haku_language");

        builder.HasIndex(t => t.LanguageCode)
            .HasDatabaseName("idx_haku_translations_language");

        // Note: Global query filter for soft delete and multi-tenancy
        // is applied in ApplicationDbContext.ApplyMultiTenancyFilter()
    }
}
