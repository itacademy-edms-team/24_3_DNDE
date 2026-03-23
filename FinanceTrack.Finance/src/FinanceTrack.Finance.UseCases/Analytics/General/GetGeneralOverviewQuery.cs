using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.General;

public sealed record GetGeneralOverviewQuery(string UserId, DateOnly From, DateOnly To)
    : IQuery<OverviewAnalyticsDto>;

public sealed class GetOverviewHandler(IGeneralAnalyticsQueryService service)
    : IQueryHandler<GetGeneralOverviewQuery, OverviewAnalyticsDto>
{
    public async Task<OverviewAnalyticsDto> Handle(
        GetGeneralOverviewQuery request,
        CancellationToken ct
    )
    {
        return await service.GetOverview(request.UserId, request.From, request.To, ct);
    }
}
