namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetCategoriesAnalyticsQuery(string UserId, DateOnly From, DateOnly To)
    : IQuery<CategoriesAnalyticsDto>;

public sealed class GetCategoriesAnalyticsHandler(IAnalyticsQueryService _service)
    : IQueryHandler<GetCategoriesAnalyticsQuery, CategoriesAnalyticsDto>
{
    public async Task<CategoriesAnalyticsDto> Handle(GetCategoriesAnalyticsQuery request, CancellationToken ct)
    {
        return await _service.GetCategoriesAnalytics(request.UserId, request.From, request.To, ct);
    }
}
