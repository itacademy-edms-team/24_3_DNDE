using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetWalletYearMinMaxQuery(string userId, Guid WalletId)
    : IQuery<Result<YearMinMaxDto>>;

public sealed class GetWalletYearMinMaxQueryHandler(IWalletMetadataQueryService service)
    : IQueryHandler<GetWalletYearMinMaxQuery, Result<YearMinMaxDto>>
{
    public async Task<Result<YearMinMaxDto>> Handle(
        GetWalletYearMinMaxQuery request,
        CancellationToken cancellationToken
    )
    {
        return await service.GetDateMinMax(request.userId, request.WalletId, cancellationToken);
    }
}
