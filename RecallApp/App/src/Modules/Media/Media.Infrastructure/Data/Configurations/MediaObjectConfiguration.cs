using Media.Domain.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

internal sealed class MediaObjectConfiguration : IEntityTypeConfiguration<MediaObject>
{
    public void Configure(EntityTypeBuilder<MediaObject> builder)
    {
        builder.ToTable("media_objects");

        builder.HasKey(media => media.Id);

        builder.Property(media => media.OwnerId).IsRequired();
        builder.Property(media => media.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(media => media.DeclaredContentType).HasMaxLength(64).IsRequired();
        builder.Property(media => media.SourceFormat).HasConversion<string>().HasMaxLength(16);
        builder.Property(media => media.FailureCode).HasMaxLength(64);
        builder.Property(media => media.FailureDescription).HasMaxLength(512);
        builder.Property(media => media.UploadExpiresAt).IsRequired();
        builder.Property(media => media.CreatedAt).IsRequired();
        builder.Property(media => media.UpdatedAt).IsRequired();

        // Список своих файлов: свежие первыми.
        builder.HasIndex(media => new { media.OwnerId, media.CreatedAt })
            .HasDatabaseName("ix_media_objects_owner_id_created_at")
            .IsDescending(false, true);

        // Фоновая чистка брошенных загрузок и недоступных записей.
        builder.HasIndex(media => new { media.Status, media.CreatedAt })
            .HasDatabaseName("ix_media_objects_status_created_at");

        // Готовые объекты без ссылок
        builder.HasIndex(media => new { media.Status, media.ReadyAt })
            .HasDatabaseName("ix_media_objects_status_ready_at");

        builder.HasMany(media => media.Variants)
            .WithOne()
            .HasForeignKey(variant => variant.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(media => media.Variants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_variants");
    }
}
