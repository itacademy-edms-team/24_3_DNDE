using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.Wallet;

public sealed record GetWalletCashFlowQuery(
    string UserId,
    Guid WalletId,
    DateOnly From,
    DateOnly To
) : IQuery<Result<CashFlowDto>>;

public sealed class GetWalletCashFlowHandler(IWalletAnalyticsQueryService service)
    : IQueryHandler<GetWalletCashFlowQuery, Result<CashFlowDto>>
{
    public async Task<Result<CashFlowDto>> Handle(
        GetWalletCashFlowQuery request,
        CancellationToken ct
    )
    {
        return await service.GetWalletCashFlow(
            request.UserId,
            request.WalletId,
            request.From,
            request.To,
            ct
        );
    }
}
