namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetAccountAnalyticsQuery(
    string UserId,
    Guid WalletId,
    DateOnly From,
    DateOnly To
) : IQuery<Result<AccountAnalyticsDto>>;

public sealed class GetAccountAnalyticsHandler(IAnalyticsQueryService _service)
    : IQueryHandler<GetAccountAnalyticsQuery, Result<AccountAnalyticsDto>>
{
    public async Task<Result<AccountAnalyticsDto>> Handle(
        GetAccountAnalyticsQuery request,
        CancellationToken ct
    )
    {
        try
        {
            var result = await _service.GetAccountAnalytics(
                request.UserId,
                request.WalletId,
                request.From,
                request.To,
                ct
            );
            return Result.Success(result);
        }
        catch (InvalidOperationException)
        {
            return Result.NotFound();
        }
    }
}
