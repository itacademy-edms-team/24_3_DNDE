namespace FinanceTrack.Finance.UseCases.Wallets;

public interface IWalletForecastQueryService
{
    Task<Result<WalletForecastBalanceDto>> GetBalanceForecast(
        string userId,
        Guid walletId,
        CancellationToken ct = default
    );
}
