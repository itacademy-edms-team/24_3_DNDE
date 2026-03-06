using FinanceTrack.Finance.Core.RecurringTransactionAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
{
    public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
    {
        builder.HasKey(r => r.Id);

        builder
            .Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(FinancialTransactionDataSchemaConstants.USER_ID_MAX_LENGTH);

        builder
            .Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(FinancialTransactionDataSchemaConstants.TRANSACTION_NAME_MAX_LENGTH);

        builder
            .Property(r => r.TransactionType)
            .HasConversion(x => x.Value, x => RecurringTransactionType.FromValue(x))
            .IsRequired();

        builder.Property(r => r.Amount).HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(r => r.DayOfMonth).IsRequired();

        builder.Property(r => r.StartDate).HasColumnType("date").IsRequired();

        builder.Property(r => r.EndDate).HasColumnType("date");

        builder.Property(r => r.IsActive).HasDefaultValue(true).IsRequired();

        builder.Property(r => r.LastProcessedDate).HasColumnType("date");

        builder
            .Property(r => r.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            .IsRequired();

        builder
            .HasOne(r => r.Wallet)
            .WithMany()
            .HasForeignKey(r => r.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.WalletId);
        builder.HasIndex(r => r.IsActive);
    }
}
