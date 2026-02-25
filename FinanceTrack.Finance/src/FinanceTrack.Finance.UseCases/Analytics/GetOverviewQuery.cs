namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetOverviewQuery(string UserId, DateOnly From, DateOnly To)
    : IQuery<OverviewAnalyticsDto>;

public sealed class GetOverviewHandler(IAnalyticsQueryService _service)
    : IQueryHandler<GetOverviewQuery, OverviewAnalyticsDto>
{
    public async Task<OverviewAnalyticsDto> Handle(GetOverviewQuery request, CancellationToken ct)
    {
        return await _service.GetOverview(request.UserId, request.From, request.To, ct);
    }
}
