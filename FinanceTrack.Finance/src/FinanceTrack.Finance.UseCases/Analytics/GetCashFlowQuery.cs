namespace FinanceTrack.Finance.UseCases.Analytics;

public sealed record GetCashFlowQuery(string UserId, DateOnly From, DateOnly To)
    : IQuery<CashFlowDto>;

public sealed class GetCashFlowHandler(IAnalyticsQueryService _service)
    : IQueryHandler<GetCashFlowQuery, CashFlowDto>
{
    public async Task<CashFlowDto> Handle(GetCashFlowQuery request, CancellationToken ct)
    {
        return await _service.GetCashFlow(request.UserId, request.From, request.To, ct);
    }
}
