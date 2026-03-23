using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.Wallet;

public sealed record GetWalletOverviewQuery(
    string UserId,
    Guid WalletId,
    DateOnly From,
    DateOnly To
) : IQuery<Result<WalletOverviewDto>>;

public sealed class GetWalletOverviewHandler(IWalletAnalyticsQueryService service)
    : IQueryHandler<GetWalletOverviewQuery, Result<WalletOverviewDto>>
{
    public async Task<Result<WalletOverviewDto>> Handle(
        GetWalletOverviewQuery request,
        CancellationToken ct
    )
    {
        return await service.GetWalletOverview(
            request.UserId,
            request.WalletId,
            request.From,
            request.To,
            ct
        );
    }
}
