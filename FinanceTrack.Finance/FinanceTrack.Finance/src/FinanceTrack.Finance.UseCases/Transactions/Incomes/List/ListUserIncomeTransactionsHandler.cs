using FinanceTrack.Finance.Core.TransactionAggregate;
using FinanceTrack.Finance.Core.TransactionAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.List;

public sealed class ListUserIncomeTransactionsHandler(IReadRepository<Transaction> repository)
    : IQueryHandler<ListUserIncomeTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    public async Task<IReadOnlyList<TransactionDto>> Handle(
        ListUserIncomeTransactionsQuery request,
        CancellationToken cancellationToken
    )
    {
        var spec = new UserIncomeTransactionsSpec(request.UserId);

        var items = await repository.ListAsync(spec, cancellationToken);

        return items
            .Select(t => new TransactionDto(
                t.Id,
                t.Name,
                t.Amount,
                t.OperationDate,
                t.IsMonthly,
                t.TransactionType.Name
            ))
            .ToList();
    }
}
