using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using NpgsqlTypes;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .Property(t => t.UserId)
            .IsRequired()
            .HasMaxLength(FinancialTransactionDataSchemaConstants.UserIdMaxLength);

        builder
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(FinancialTransactionDataSchemaConstants.TransactionNameMaxLength);

        builder
            .Property(t => t.Description)
            .HasMaxLength(FinancialTransactionDataSchemaConstants.TransactionDescriptionMaxLength);

        builder
            .Property(t => t.TransactionType)
            .HasConversion(x => x.Value, x => FinancialTransactionType.FromValue(x))
            .IsRequired();

        builder.Property(t => t.Amount).HasColumnType("numeric(18,2)").IsRequired();

        builder.Property(t => t.OperationDate).HasColumnType("date").IsRequired();

        builder
            .Property(t => t.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            .IsRequired();

        // FK to Wallet
        builder
            .HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Category (optional)
        builder
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Self-reference for transfer pairs
        builder.Property(t => t.RelatedTransactionId);

        // Reference to recurring rule
        builder.Property(t => t.RecurringTransactionId);

        // Full text search field
        builder
            .Property<NpgsqlTsVector>("SearchVector")
            .HasColumnType("tsvector")
            .HasComputedColumnSql(
                $"to_tsvector('{DbConstants.FullTextSearchLanguage}', "
                    + "coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true
            );
        builder.HasIndex("SearchVector").HasMethod("GIN");

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.OperationDate);
        builder.HasIndex(t => t.TransactionType);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.RelatedTransactionId);
        builder.HasIndex(t => t.RecurringTransactionId);
    }
}
