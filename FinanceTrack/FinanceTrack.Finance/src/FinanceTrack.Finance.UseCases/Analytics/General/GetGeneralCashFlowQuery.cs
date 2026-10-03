using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.General;

public sealed record GetGeneralCashFlowQuery(string UserId, DateOnly From, DateOnly To)
    : IQuery<CashFlowDto>;

public sealed class GetCashFlowHandler(IGeneralAnalyticsQueryService service)
    : IQueryHandler<GetGeneralCashFlowQuery, CashFlowDto>
{
    public async Task<CashFlowDto> Handle(GetGeneralCashFlowQuery request, CancellationToken cancel)
    {
        return await service.GetCashFlow(request.UserId, request.From, request.To, cancel);
    }
}
