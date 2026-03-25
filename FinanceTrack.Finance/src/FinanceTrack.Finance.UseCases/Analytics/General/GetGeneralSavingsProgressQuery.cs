using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.UseCases.Analytics.General;

public sealed record GetGeneralSavingsProgressQuery(string UserId) : IQuery<SavingsProgressDto>;

public sealed class GetSavingsProgressHandler(IGeneralAnalyticsQueryService service)
    : IQueryHandler<GetGeneralSavingsProgressQuery, SavingsProgressDto>
{
    public async Task<SavingsProgressDto> Handle(
        GetGeneralSavingsProgressQuery request,
        CancellationToken cancel
    )
    {
        return await service.GetSavingsProgress(request.UserId, cancel);
    }
}
