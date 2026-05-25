using FinanceTrack.Finance.Core.WalletInsightAggregate;
using FinanceTrack.Finance.Core.WalletInsightAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.WalletInsights.Get;

public sealed class GetWalletInsightsHandler(IReadRepository<WalletInsight> repo)
    : IQueryHandler<GetWalletInsightsQuery, Result<WalletInsightDto>>
{
    public async Task<Result<WalletInsightDto>> Handle(
        GetWalletInsightsQuery request,
        CancellationToken cancel
    )
    {
        var spec = new WalletInsightByWalletMonthSpec(
            request.UserId,
            request.WalletId,
            request.InsightMonth
        );

        var insight = await repo.FirstOrDefaultAsync(spec, cancel);

        if (insight is null)
            return Result.NotFound();

        return Result.Success(
            new WalletInsightDto(
                insight.AnomaliesText,
                insight.TrendsText,
                insight.ExpenseStructureText,
                insight.RecommendationsText,
                insight.GeneratedAtUtc
            )
        );
    }
}
