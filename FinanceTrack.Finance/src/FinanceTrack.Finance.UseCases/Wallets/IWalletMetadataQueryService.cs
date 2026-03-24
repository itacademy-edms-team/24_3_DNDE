using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Wallets;

public interface IWalletMetadataQueryService
{
    Task<DateMinMaxDto> GetDateMinMax(string userId, CancellationToken ct = default);
    Task<Result<DateMinMaxDto>> GetDateMinMax(
        string userId,
        Guid walletId,
        CancellationToken ct = default
    );
}
