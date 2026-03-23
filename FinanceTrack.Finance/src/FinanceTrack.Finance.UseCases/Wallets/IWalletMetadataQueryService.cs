using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Wallets;

public interface IWalletMetadataQueryService
{
    Task<Result<YearMinMaxDto>> GetDateMinMax(string userId, CancellationToken ct = default);
    Task<Result<YearMinMaxDto>> GetDateMinMax(
        string userId,
        Guid WalletId,
        CancellationToken ct = default
    );
}
