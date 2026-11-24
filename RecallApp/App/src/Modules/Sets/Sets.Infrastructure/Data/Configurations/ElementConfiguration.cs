using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Infrastructure.Data.Configurations;

internal sealed class ElementConfiguration : IEntityTypeConfiguration<Element>
{
    public void Configure(EntityTypeBuilder<Element> builder)
    {
        builder.ToTable("elements");
        builder.HasKey(element => element.Id);

        builder.Property(element => element.Id).HasColumnName("id");
        builder.Property(element => element.SetId).HasColumnName("set_id").IsRequired();

        builder.Property(element => element.Type)
            .HasColumnName("type")
            .HasMaxLength(32)
            .HasConversion(
                type => type.ToContractValue(),
                value => ParseStoredType(value))
            .IsRequired();

        // Строки сортируются побайтово (COLLATE "C"), чтобы правильно работало сравнение
        builder.Property(element => element.Position)
            .HasColumnName("position")
            .HasColumnType("text")
            .UseCollation("C")
            .IsRequired();

        // Документное поле типа (ADR 0015): реляционная часть общая, состав payload задаёт тип.
        // Один конвертер на один тип. Новый тип - новый конвертер и добавление новго HasConversion.
        builder.Property(element => element.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .HasConversion(new ValueConverter<ElementPayload, string>(
                payload => ElementPayloadJson.ToStorageJson(payload),
                json => ElementPayloadJson.FromStorageJson(json)))
            .IsRequired();

        builder.Property(element => element.CreatedAt).HasColumnName("created_at");
        builder.Property(element => element.UpdatedAt).HasColumnName("updated_at");
        builder.Property(element => element.DeletedAt).HasColumnName("deleted_at");
        
        // Элемент не может существовать вне набора
        builder.HasOne<Set>()
            .WithMany()
            .HasForeignKey(element => element.SetId)
            .OnDelete(DeleteBehavior.Cascade) // с удалением набора удаляем и привязанные к нему элементы
            .HasConstraintName("fk_elements_sets_set_id");

        // Уникальные ранги среди активных элементов: конфликт ловится базой, а не логикой.
        builder.HasIndex(element => new { element.SetId, element.Position })
            .IsUnique()
            .HasDatabaseName("ux_elements_set_id_position")
            .HasFilter("deleted_at IS NULL");

        // Курсорная выдача в порядке (position, id) (ADR 0039).
        builder.HasIndex(element => new { element.SetId, element.Position, element.Id })
            .HasDatabaseName("ix_elements_set_id_position_id")
            .HasFilter("deleted_at IS NULL");
    }

    private static ElementType ParseStoredType(string value) =>
        ElementTypeNames.TryParse(value, out var type)
            ? type
            : throw new InvalidOperationException($"Unknown element type '{value}' stored in sets.elements.");
}
