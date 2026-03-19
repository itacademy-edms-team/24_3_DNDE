using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.List;

public sealed class ListUserRecurringTransactionsHandler(IReadRepository<RecurringTransaction> repo)
    : IQueryHandler<ListUserRecurringTransactionsQuery, IReadOnlyList<RecurringTransactionDto>>
{
    public async Task<IReadOnlyList<RecurringTransactionDto>> Handle(
        ListUserRecurringTransactionsQuery request,
        CancellationToken ct
    )
    {
        var spec = new UserRecurringTransactionsSpec(request.UserId);
        var items = await repo.ListAsync(spec, ct);

        return items
            .Select(r => new RecurringTransactionDto(
                r.Id,
                r.WalletId,
                r.CategoryId,
                r.Name,
                r.Description,
                r.TransactionType.Name,
                r.Amount,
                r.DayOfMonth,
                r.StartDate,
                r.EndDate,
                r.IsActive,
                r.LastProcessedDate
            ))
            .ToList();
    }
}
