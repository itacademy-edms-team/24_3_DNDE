using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics;

public interface IGeneralAnalyticsQueryService
{
    Task<OverviewAnalyticsDto> GetOverview(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    );

    Task<CashFlowDto> GetCashFlow(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    );

    Task<SavingsProgressDto> GetSavingsProgress(string userId, CancellationToken ct = default);

    Task<CategoriesAnalyticsDto> GetCategoriesAnalytics(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancel = default
    );
}
