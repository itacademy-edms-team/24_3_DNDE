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

        builder.Property(media => media.Id)
            .HasColumnName("id");
        builder.Property(media => media.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();
        builder.Property(media => media.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(media => media.DeclaredContentType)
            .HasColumnName("declared_content_type")
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(media => media.SourceFormat)
            .HasColumnName("source_format")
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.Property(media => media.SourceSizeBytes)
            .HasColumnName("source_size_bytes");
        builder.Property(media => media.Width)
            .HasColumnName("width");
        builder.Property(media => media.Height)
            .HasColumnName("height");
        builder.Property(media => media.FailureCode)
            .HasColumnName("failure_code")
            .HasMaxLength(64);
        builder.Property(media => media.FailureDescription)
            .HasColumnName("failure_description")
            .HasMaxLength(512);
        builder.Property(media => media.UploadExpiresAt)
            .HasColumnName("upload_expires_at")
            .IsRequired();
        builder.Property(media => media.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(media => media.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        builder.Property(media => media.ReadyAt)
            .HasColumnName("ready_at");
        builder.Property(media => media.DeletedAt)
            .HasColumnName("deleted_at");

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
