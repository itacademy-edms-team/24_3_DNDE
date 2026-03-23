using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetYearMinMaxQuery(string UserId) : IQuery<Result<YearMinMaxDto>>;

public sealed class GetYearMinMaxQueryHandler(IWalletMetadataQueryService service)
    : IQueryHandler<GetYearMinMaxQuery, Result<YearMinMaxDto>>
{
    public async Task<Result<YearMinMaxDto>> Handle(
        GetYearMinMaxQuery request,
        CancellationToken ct
    )
    {
        return await service.GetDateMinMax(request.UserId, ct);
    }
}
