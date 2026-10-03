using FinanceTrack.Finance.UseCases.WalletInsights;

namespace FinanceTrack.Finance.UseCases.WalletInsights.Generate;

public sealed record GenerateWalletInsightsCommand(string UserId, Guid WalletId)
    : ICommand<Result<WalletInsightDto>>;
