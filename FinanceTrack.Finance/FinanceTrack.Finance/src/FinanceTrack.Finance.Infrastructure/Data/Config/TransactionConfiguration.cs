using FinanceTrack.Finance.Core.ContributorAggregate;
using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
  public void Configure(EntityTypeBuilder<Transaction> builder)
  {
    builder.HasKey(t => t.Id);

    builder
      .Property(t => t.UserId)
      .IsRequired()
      .HasMaxLength(TransactionDataSchemaConstants.USER_ID_MAX_LENGTH);

    builder
      .Property(t => t.TransactionType)
      .HasConversion(x => x.Value, x => TransactionType.FromValue(x))
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
