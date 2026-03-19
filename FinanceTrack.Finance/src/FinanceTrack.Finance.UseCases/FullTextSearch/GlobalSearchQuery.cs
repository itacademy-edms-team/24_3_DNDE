namespace FinanceTrack.Finance.UseCases.FullTextSearch;

public sealed record GlobalSearchQuery(string UserId, string Query, int LimitPerType)
    : IQuery<Result<GlobalSearchResult>>;
