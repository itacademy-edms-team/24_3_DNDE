using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.UseCases.Analytics.Dto;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

internal static class CategoryBreakdownHelper
{
    internal static List<CategoryBreakdownDto> Build(
        List<FinancialTransaction> transactions,
        FinancialTransactionType type,
        Dictionary<Guid, string> categoryDict
    )
    {
        var filtered = transactions.Where(t => t.TransactionType == type).ToList();
        var total = filtered.Sum(t => t.Amount);
        var divisor = total > 0 ? total : 1m;

        return filtered
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var amount = g.Sum(t => t.Amount);
                return new CategoryBreakdownDto(
                    g.Key,
                    g.Key.HasValue && categoryDict.TryGetValue(g.Key.Value, out var name)
                        ? name
                        : null,
                    amount,
                    Math.Round(amount / divisor * 100, 2)
                );
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }
}
