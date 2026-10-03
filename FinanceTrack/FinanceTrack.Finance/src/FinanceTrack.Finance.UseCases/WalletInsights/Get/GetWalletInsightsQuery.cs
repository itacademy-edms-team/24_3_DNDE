using FinanceTrack.Finance.UseCases.WalletInsights;

namespace FinanceTrack.Finance.UseCases.WalletInsights.Get;

public sealed record GetWalletInsightsQuery(string UserId, Guid WalletId, DateOnly InsightMonth)
    : IQuery<Result<WalletInsightDto>>;
