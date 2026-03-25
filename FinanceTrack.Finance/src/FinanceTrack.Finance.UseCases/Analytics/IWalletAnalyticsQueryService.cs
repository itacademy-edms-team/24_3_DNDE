using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics;

public interface IWalletAnalyticsQueryService
{
    Task<Result<WalletOverviewDto>> GetWalletOverview(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    );

    Task<Result<CashFlowDto>> GetWalletCashFlow(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    );

    Task<Result<CategoriesAnalyticsDto>> GetWalletCategoriesAnalytics(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    );
}
