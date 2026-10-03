using FinanceTrack.Finance.Core.WalletAggregate;
using NpgsqlTypes;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder
            .Property(w => w.UserId)
            .IsRequired()
            .HasMaxLength(WalletDataSchemaConstants.UserIdMaxLength);

        builder
            .Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(WalletDataSchemaConstants.WalletNameMaxLength);

        builder
            .Property(w => w.WalletType)
            .HasConversion(x => x.Value, x => WalletType.FromValue(x))
            .IsRequired();

        builder.Property(w => w.Balance).HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(w => w.AllowNegativeBalance).HasDefaultValue(true).IsRequired();

        builder.Property(w => w.TargetAmount).HasColumnType("numeric(18,2)");

        builder.Property(w => w.TargetDate).HasColumnType("date");

        builder.Property(w => w.IsArchived).HasDefaultValue(false).IsRequired();

        builder
            .Property(w => w.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            .IsRequired();

        // Full text search field
        builder
            .Property<NpgsqlTsVector>("SearchVector")
            .HasColumnType("tsvector")
            .HasComputedColumnSql("to_tsvector('russian', coalesce(\"Name\", ''))", stored: true);
        builder.HasIndex("SearchVector").HasMethod("GIN");

        builder.HasIndex(w => w.UserId);
        builder.HasIndex(w => w.WalletType);
    }
}
