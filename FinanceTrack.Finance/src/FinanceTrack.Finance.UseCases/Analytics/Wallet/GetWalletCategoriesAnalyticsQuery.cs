using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.Wallet;

public sealed record GetWalletCategoriesAnalyticsQuery(
    string UserId,
    Guid WalletId,
    DateOnly From,
    DateOnly To
) : IQuery<Result<CategoriesAnalyticsDto>>;

public sealed class GetWalletCategoriesAnalyticsHandler(IWalletAnalyticsQueryService service)
    : IQueryHandler<GetWalletCategoriesAnalyticsQuery, Result<CategoriesAnalyticsDto>>
{
    public async Task<Result<CategoriesAnalyticsDto>> Handle(
        GetWalletCategoriesAnalyticsQuery request,
        CancellationToken ct
    )
    {
        return await service.GetWalletCategoriesAnalytics(
            request.UserId,
            request.WalletId,
            request.From,
            request.To,
            ct
        );
    }
}
