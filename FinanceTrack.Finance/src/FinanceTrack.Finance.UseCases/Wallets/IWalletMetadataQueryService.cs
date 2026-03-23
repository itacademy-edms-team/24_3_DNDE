using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Wallets;

public interface IWalletMetadataQueryService
{
    Task<YearMinMaxDto> GetDateMinMax(string userId, CancellationToken ct = default);
    Task<Result<YearMinMaxDto>> GetDateMinMax(
        string userId,
        Guid walletId,
        CancellationToken ct = default
    );
}
