using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetWalletDateMinMaxQuery(string userId, Guid WalletId)
    : IQuery<Result<DateMinMaxDto>>;

public sealed class GetWalletDateMinMaxQueryHandler(IWalletMetadataQueryService service)
    : IQueryHandler<GetWalletDateMinMaxQuery, Result<DateMinMaxDto>>
{
    public async Task<Result<DateMinMaxDto>> Handle(
        GetWalletDateMinMaxQuery request,
        CancellationToken cancel
    )
    {
        return await service.GetDateMinMax(request.userId, request.WalletId, cancel);
    }
}
