namespace FinanceTrack.Finance.UseCases.WalletInsights;

public sealed record WalletInsightDto(
    string? AnomaliesText,
    string? TrendsText,
    string? ExpenseStructureText,
    string? RecommendationsText,
    DateTime GeneratedAtUtc
);
