using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.List;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class ListUserIncomeFinancialTransactionsQueryService(
    IRepository<FinancialTransaction> _repository
) : IListUserIncomeFinancialTransactionsQueryService
{
    public async Task<Result<IReadOnlyList<FinancialTransactionDto>>> GetUserIncomeTransactions(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var spec = new UserIncomeFinancialTransactionsSpec(userId);

        var items = await _repository.ListAsync(spec, cancellationToken);

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
