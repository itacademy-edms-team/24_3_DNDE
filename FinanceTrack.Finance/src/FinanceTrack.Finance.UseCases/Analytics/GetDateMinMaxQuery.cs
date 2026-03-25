using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetDateMinMaxQuery(string UserId) : IQuery<DateMinMaxDto>;

public sealed class GetDateMinMaxQueryHandler(IWalletMetadataQueryService service)
    : IQueryHandler<GetDateMinMaxQuery, DateMinMaxDto>
{
    public async Task<DateMinMaxDto> Handle(GetDateMinMaxQuery request, CancellationToken cancel)
    {
        return await service.GetDateMinMax(request.UserId, cancel);
    }
}
