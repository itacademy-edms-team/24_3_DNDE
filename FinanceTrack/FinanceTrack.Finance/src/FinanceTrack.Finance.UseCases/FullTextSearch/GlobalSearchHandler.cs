namespace FinanceTrack.Finance.UseCases.FullTextSearch;

public sealed class GlobalSearchHandler(IGlobalFullTextSearchQueryService service)
    : IQueryHandler<GlobalSearchQuery, Result<GlobalSearchResult>>
{
    public async Task<Result<GlobalSearchResult>> Handle(
        GlobalSearchQuery request,
        CancellationToken ct
    )
    {
        return await service.SearchAsync(request.UserId, request.Query, request.LimitPerType, ct);
    }
}
