using FinanceTrack.Finance.Core.WalletInsightAggregate;

namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record MonthlyFlowData(
    int Year,
    int Month,
    decimal Income,
    decimal Expense,
    decimal NetFlow
);

public sealed record CategoryAmountData(string CategoryName, decimal Amount, decimal Percentage);

public sealed record WalletGoalData(
    decimal CurrentBalance,
    decimal TargetAmount,
    DateOnly? TargetDate,
    decimal AverageMonthlyNetFlow,
    decimal MonthsToGoal
);

public sealed record WalletInsightsInput(
    IReadOnlyList<MonthlyFlowData> MonthlyFlows,
    IReadOnlyList<CategoryAmountData> CurrentMonthExpenses,
    IReadOnlyList<CategoryAmountData> PriorMonthsAverageExpenses,
    WalletGoalData? Goal
);

public interface IWalletInsightsAiService
{
    bool IsEnabled { get; }

    Task<WalletInsightsAiResult> GenerateInsightsAsync(
        WalletInsightsInput input,
        CancellationToken cancel
    );
}
