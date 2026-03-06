using FinanceTrack.Finance.Core.CategoryAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder
            .Property(c => c.UserId)
            .IsRequired()
            .HasMaxLength(CategoryDataSchemaConstants.USER_ID_MAX_LENGTH);

        builder
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(CategoryDataSchemaConstants.CATEGORY_NAME_MAX_LENGTH);

        builder
            .Property(c => c.Type)
            .HasConversion(x => x.Value, x => CategoryType.FromValue(x))
            .IsRequired();

        builder.Property(c => c.Icon).HasMaxLength(CategoryDataSchemaConstants.ICON_MAX_LENGTH);

        builder.Property(c => c.Color).HasMaxLength(CategoryDataSchemaConstants.COLOR_MAX_LENGTH);

        builder
            .Property(c => c.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            .IsRequired();

        builder.HasIndex(c => c.UserId);
    }
}
