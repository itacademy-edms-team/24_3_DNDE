namespace FinanceTrack.Finance.UseCases.Analytics;

public interface IAnalyticsQueryService
{
    Task<OverviewAnalyticsDto> GetOverview(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    );

    Task<AccountAnalyticsDto> GetAccountAnalytics(
        string userId,
        Guid walletId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    );

    Task<CashFlowDto> GetCashFlow(
        string userId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    );

    Task<SavingsProgressDto> GetSavingsProgress(
        string userId,
        CancellationToken ct = default
    );
}
