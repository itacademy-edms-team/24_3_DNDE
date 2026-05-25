using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletInsightAggregate;
using FinanceTrack.Finance.Core.WalletInsightAggregate.Specifications;
using FinanceTrack.Finance.UseCases.Analytics;

namespace FinanceTrack.Finance.UseCases.WalletInsights.Generate;

public sealed class GenerateWalletInsightsHandler(
    IReadRepository<Wallet> walletRepo,
    IRepository<WalletInsight> insightRepo,
    IUnitOfWork unitOfWork,
    IWalletAnalyticsQueryService analyticsService,
    IWalletInsightsAiService aiService
) : ICommandHandler<GenerateWalletInsightsCommand, Result<WalletInsightDto>>
{
    public async Task<Result<WalletInsightDto>> Handle(
        GenerateWalletInsightsCommand request,
        CancellationToken cancel
    )
    {
        var wallet = await walletRepo.FirstOrDefaultAsync(
            new WalletByIdSpec(request.WalletId),
            cancel
        );

        if (wallet is null)
            return Result.NotFound("Wallet not found.");

        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var insightMonth = new DateOnly(today.Year, today.Month, 1);
        var currentMonthEnd = new DateOnly(
            today.Year,
            today.Month,
            DateTime.DaysInMonth(today.Year, today.Month)
        );

        // Last 4 months: 3 prior + current. Used for Trends and Recommendations avg.
        var prior3Start = insightMonth.AddMonths(-3);
        var prior3End = insightMonth.AddDays(-1);

        var currentCategoriesResult = await analyticsService.GetWalletCategoriesAnalytics(
            request.UserId,
            request.WalletId,
            insightMonth,
            currentMonthEnd,
            cancel
        );

        if (!currentCategoriesResult.IsSuccess)
            return Result.Error("Failed to load current month analytics.");

        var priorCategoriesResult = await analyticsService.GetWalletCategoriesAnalytics(
            request.UserId,
            request.WalletId,
            prior3Start,
            prior3End,
            cancel
        );

        if (!priorCategoriesResult.IsSuccess)
            return Result.Error("Failed to load prior months analytics.");

        var cashFlowResult = await analyticsService.GetWalletCashFlow(
            request.UserId,
            request.WalletId,
            prior3Start,
            currentMonthEnd,
            cancel
        );

        if (!cashFlowResult.IsSuccess)
            return Result.Error("Failed to load cash flow data.");

        var currentExpenses = currentCategoriesResult
            .Value.ExpenseByCategory.Where(c => c.CategoryName is not null)
            .Select(c => new CategoryAmountData(c.CategoryName!, c.Amount, c.Percentage))
            .ToList();

        // Prior 3 months totals → average by dividing by 3
        var priorExpenses = priorCategoriesResult
            .Value.ExpenseByCategory.Where(c => c.CategoryName is not null)
            .Select(c => new CategoryAmountData(c.CategoryName!, c.Amount / 3m, c.Percentage))
            .ToList();

        var monthlyFlows = cashFlowResult
            .Value.Periods.Select(p => new MonthlyFlowData(
                p.Year,
                p.Month,
                p.Income,
                p.Expense,
                p.Net
            ))
            .ToList();

        WalletGoalData? goal = null;
        if (wallet.WalletType == WalletType.Savings && wallet.TargetAmount.HasValue)
        {
            var priorFlows = cashFlowResult
                .Value.Periods.Where(p => !(p.Year == today.Year && p.Month == today.Month))
                .ToList();

            var avgNetFlow = priorFlows.Count > 0 ? priorFlows.Average(p => p.Net) : 0m;

            var remaining = wallet.TargetAmount.Value - wallet.Balance;
            var monthsToGoal =
                avgNetFlow > 0 && remaining > 0 ? Math.Ceiling(remaining / avgNetFlow) : 0m;

            goal = new WalletGoalData(
                wallet.Balance,
                wallet.TargetAmount.Value,
                wallet.TargetDate,
                avgNetFlow,
                monthsToGoal
            );
        }

        var input = new WalletInsightsInput(monthlyFlows, currentExpenses, priorExpenses, goal);
        var aiResult = await aiService.GenerateInsightsAsync(input, cancel);

        var spec = new WalletInsightByWalletMonthSpec(
            request.UserId,
            request.WalletId,
            insightMonth
        );
        var existing = await insightRepo.FirstOrDefaultAsync(spec, cancel);

        if (existing is null)
        {
            var created = WalletInsight.Create(
                request.UserId,
                request.WalletId,
                insightMonth,
                aiResult
            );
            await insightRepo.AddAsync(created, cancel);
        }
        else
        {
            existing.Regenerate(aiResult);
            await insightRepo.UpdateAsync(existing, cancel);
        }

        await unitOfWork.SaveChangesAsync(cancel);

        return Result.Success(
            new WalletInsightDto(
                aiResult.AnomaliesText,
                aiResult.TrendsText,
                aiResult.ExpenseStructureText,
                aiResult.RecommendationsText,
                DateTime.UtcNow
            )
        );
    }
}
