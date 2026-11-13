using Media.Domain.Objects;
using Media.Domain.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Configurations;

internal sealed class MediaReferenceConfiguration : IEntityTypeConfiguration<MediaReference>
{
    public void Configure(EntityTypeBuilder<MediaReference> builder)
    {
        builder.ToTable("media_references");

        builder.HasKey(reference => reference.Id);

        builder.Property(reference => reference.Id)
            .HasColumnName("id");
        builder.Property(reference => reference.MediaId)
            .HasColumnName("media_id")
            .IsRequired();
        builder.Property(reference => reference.Reference)
            .HasColumnName("reference")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(reference => reference.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(reference => reference.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Слот существует в одном экземпляре: перезапись вместо накопления дублей.
        builder.HasIndex(reference => reference.Reference)
            .HasDatabaseName("ux_media_references_reference")
            .IsUnique();

        // Подсчёт ссылок при решении об удалении.
        builder.HasIndex(reference => reference.MediaId)
            .HasDatabaseName("ix_media_references_media_id");

        builder.HasOne<MediaObject>()
            .WithMany()
            .HasForeignKey(reference => reference.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
