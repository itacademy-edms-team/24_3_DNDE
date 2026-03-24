namespace FinanceTrack.Finance.UseCases.Wallets.GetForecastBalance;

public sealed class GetForecastBalanceHandler(IWalletForecastQueryService service)
    : IQueryHandler<GetForecastBalanceQuery, Result<WalletForecastBalanceDto>>
{
    public async Task<Result<WalletForecastBalanceDto>> Handle(
        GetForecastBalanceQuery request,
        CancellationToken ct
    )
    {
        return await service.GetBalanceForecast(request.UserId, request.WalletId, ct);
    }
}
