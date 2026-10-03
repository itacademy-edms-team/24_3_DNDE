using FinanceTrack.Finance.Core.Shared;

namespace FinanceTrack.Finance.Core.WalletInsightAggregate;

public sealed class WalletInsight : GuidEntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = default!;
    public Guid WalletId { get; private set; }
    public DateOnly InsightMonth { get; private set; }
    public string? AnomaliesText { get; private set; }
    public string? TrendsText { get; private set; }
    public string? ExpenseStructureText { get; private set; }
    public string? RecommendationsText { get; private set; }
    public DateTime GeneratedAtUtc { get; private set; }

    // ORM
    private WalletInsight() { }

    public static WalletInsight Create(
        string userId,
        Guid walletId,
        DateOnly insightMonth,
        WalletInsightsAiResult result
    )
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.Default(walletId);

        return new WalletInsight
        {
            UserId = userId,
            WalletId = walletId,
            InsightMonth = insightMonth,
            AnomaliesText = result.AnomaliesText,
            TrendsText = result.TrendsText,
            ExpenseStructureText = result.ExpenseStructureText,
            RecommendationsText = result.RecommendationsText,
            GeneratedAtUtc = DateTime.UtcNow,
        };
    }

    public void Regenerate(WalletInsightsAiResult result)
    {
        AnomaliesText = result.AnomaliesText;
        TrendsText = result.TrendsText;
        ExpenseStructureText = result.ExpenseStructureText;
        RecommendationsText = result.RecommendationsText;
        GeneratedAtUtc = DateTime.UtcNow;
    }
}
