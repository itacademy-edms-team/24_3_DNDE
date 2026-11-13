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

        builder.Property(variant => variant.Variant)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(variant => variant.ObjectKey)
            .HasConversion(key => key.Value, value => ObjectKey.Restore(value))
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(variant => variant.Width).IsRequired();
        builder.Property(variant => variant.Height).IsRequired();
        builder.Property(variant => variant.SizeBytes).IsRequired();
        builder.Property(variant => variant.CreatedAt).IsRequired();
    }
}
