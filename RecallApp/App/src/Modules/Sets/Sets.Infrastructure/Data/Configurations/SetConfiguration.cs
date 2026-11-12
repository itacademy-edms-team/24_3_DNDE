using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sets.Domain.Sets;

namespace Sets.Infrastructure.Data.Configurations;

internal sealed class SetConfiguration : IEntityTypeConfiguration<Set>
{
    public void Configure(EntityTypeBuilder<Set> builder)
    {
        builder.ToTable("sets");
        builder.HasKey(set => set.Id);

        builder.Property(set => set.Id).HasColumnName("id");
        builder.Property(set => set.OwnerId).HasColumnName("owner_id").IsRequired();

        builder.Property(set => set.Title)
            .HasColumnName("title")
            .HasMaxLength(SetConstraints.TitleMaxLength)
            .HasConversion(new ValueConverter<SetTitle, string>(title => title.Value, value => SetTitle.FromStorage(value)))
            .IsRequired();

        // Описание необязательно: конвертер объявлен на не-nullable типе, null EF пропускает мимо конвертера.
        builder.Property(set => set.Description)
            .HasColumnName("description")
            .HasMaxLength(SetConstraints.DescriptionMaxLength)
            .HasConversion(new ValueConverter<SetDescription, string>(
                description => description.Value,
                value => SetDescription.FromStorage(value)));

        builder.Property(set => set.CreatedAt).HasColumnName("created_at");
        builder.Property(set => set.UpdatedAt).HasColumnName("updated_at");
        builder.Property(set => set.DeletedAt).HasColumnName("deleted_at");

        // Список «мои наборы»: активные строки владельца, свежие сверху.
        builder.HasIndex(set => new { set.OwnerId, set.UpdatedAt })
            .HasDatabaseName("ix_sets_owner_id_updated_at")
            .HasFilter("deleted_at IS NULL");
    }
}
