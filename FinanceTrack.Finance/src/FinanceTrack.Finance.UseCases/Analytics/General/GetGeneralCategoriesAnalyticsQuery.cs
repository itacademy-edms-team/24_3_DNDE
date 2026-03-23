using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.General;

public sealed record GetGeneralCategoriesAnalyticsQuery(string UserId, DateOnly From, DateOnly To)
    : IQuery<CategoriesAnalyticsDto>;

public sealed class GetCategoriesAnalyticsHandler(IGeneralAnalyticsQueryService service)
    : IQueryHandler<GetGeneralCategoriesAnalyticsQuery, CategoriesAnalyticsDto>
{
    public async Task<CategoriesAnalyticsDto> Handle(
        GetGeneralCategoriesAnalyticsQuery request,
        CancellationToken ct
    )
    {
        return await service.GetCategoriesAnalytics(request.UserId, request.From, request.To, ct);
    }
}
