using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .Property(t => t.UserId)
            .IsRequired()
            .HasMaxLength(FinancialTransactionDataSchemaConstants.USER_ID_MAX_LENGTH);

        builder
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(FinancialTransactionDataSchemaConstants.TRANSACTION_NAME_MAX_LENGTH);

        builder
            .Property(t => t.TransactionType)
            .HasConversion(x => x.Value, x => FinancialTransactionType.FromValue(x))
            .IsRequired();

        builder.Property(t => t.Amount).HasColumnType("numeric(18,2)").IsRequired();

        // DateOnly -> date
        builder.Property(t => t.OperationDate).HasColumnType("date").IsRequired();

        // IsMonthly bool NOT NULL DEFAULT false
        builder.Property(t => t.IsMonthly).HasDefaultValue(false).IsRequired();

        // CreatedAtUtc datetime2/ timestamp with time zone
        builder
            .Property(t => t.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            .IsRequired();

        // Self-reference: Expense -> Income
        builder
            .HasOne(t => t.IncomeTransaction)
            .WithMany()
            .HasForeignKey(t => t.IncomeTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.OperationDate);
        builder.HasIndex(t => t.TransactionType);
        builder.HasIndex(t => t.IncomeTransactionId);
    }
}
