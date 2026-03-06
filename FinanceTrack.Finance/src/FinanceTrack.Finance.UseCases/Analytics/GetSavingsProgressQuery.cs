namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetSavingsProgressQuery(string UserId) : IQuery<SavingsProgressDto>;

public sealed class GetSavingsProgressHandler(IAnalyticsQueryService service)
    : IQueryHandler<GetSavingsProgressQuery, SavingsProgressDto>
{
    public async Task<SavingsProgressDto> Handle(
        GetSavingsProgressQuery request,
        CancellationToken ct
    )
    {
        return await service.GetSavingsProgress(request.UserId, ct);
    }
}
