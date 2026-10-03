namespace FinanceTrack.Finance.Core.WalletInsightAggregate;

public sealed record WalletInsightsAiResult(
    string? AnomaliesText,
    string? TrendsText,
    string? ExpenseStructureText,
    string? RecommendationsText
);
