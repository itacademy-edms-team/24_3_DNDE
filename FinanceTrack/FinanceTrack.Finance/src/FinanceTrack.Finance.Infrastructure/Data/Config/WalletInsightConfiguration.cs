using FinanceTrack.Finance.Core.WalletInsightAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class WalletInsightConfiguration : IEntityTypeConfiguration<WalletInsight>
{
    public void Configure(EntityTypeBuilder<WalletInsight> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.UserId).IsRequired().HasMaxLength(256);

        builder.Property(i => i.InsightMonth).IsRequired();

        builder.Property(i => i.AnomaliesText);
        builder.Property(i => i.TrendsText);
        builder.Property(i => i.ExpenseStructureText);
        builder.Property(i => i.RecommendationsText);

        builder
            .Property(i => i.GeneratedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .HasIndex(i => new
            {
                i.WalletId,
                i.UserId,
                i.InsightMonth,
            })
            .IsUnique();

        builder.HasIndex(i => new { i.WalletId, i.UserId });
    }
}
