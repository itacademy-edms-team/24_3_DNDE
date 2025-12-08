using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.List;

public sealed class ListUserIncomeFinancialTransactionsHandler(
    IReadRepository<FinancialTransaction> repository
) : IQueryHandler<ListUserIncomeFinancialTransactionsQuery, IReadOnlyList<FinancialTransactionDto>>
{
    public async Task<IReadOnlyList<FinancialTransactionDto>> Handle(
        ListUserIncomeFinancialTransactionsQuery request,
        CancellationToken cancellationToken
    )
    {
        var spec = new UserIncomeFinancialTransactionsSpec(request.UserId);

        var items = await repository.ListAsync(spec, cancellationToken);

        return items
            .Select(t => new FinancialTransactionDto(
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
