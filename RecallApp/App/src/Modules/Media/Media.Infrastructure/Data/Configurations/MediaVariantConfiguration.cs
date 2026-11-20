using Media.Domain.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

internal sealed class MediaVariantConfiguration : IEntityTypeConfiguration<MediaVariant>
{
    public void Configure(EntityTypeBuilder<MediaVariant> builder)
    {
        builder.ToTable("media_variants");

        builder.HasKey(variant => new { variant.MediaId, variant.Variant });

        builder.Property(variant => variant.MediaId).HasColumnName("media_id");
        builder.Property(variant => variant.Variant)
            .HasColumnName("variant")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(variant => variant.ObjectKey)
            .HasColumnName("object_key")
            .HasConversion(key => key.Value, value => ObjectKey.Restore(value))
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(variant => variant.Width)
            .HasColumnName("width")
            .IsRequired();
        builder.Property(variant => variant.Height)
            .HasColumnName("height")
            .IsRequired();
        builder.Property(variant => variant.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();
        builder.Property(variant => variant.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
