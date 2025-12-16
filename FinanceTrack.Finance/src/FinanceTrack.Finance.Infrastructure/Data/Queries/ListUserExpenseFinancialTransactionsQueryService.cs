using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class ListUserExpenseFinancialTransactionsQueryService(
    IRepository<FinancialTransaction> _repository
) : IListUserExpenseFinancialTransactionsQueryService
{
    public async Task<Result<IReadOnlyList<FinancialTransactionDto>>> GetUserExpenseTransactions(
        string userId,
        Guid incomeTransactionId,
        CancellationToken cancellationToken = default
    )
    {
        var spec = new UserExpenseFinancialTransactionsByIncomeSpec(userId, incomeTransactionId);

        var items = await _repository.ListAsync(spec, cancellationToken);

        return items
            .Select(
                t =>
                    new FinancialTransactionDto(
                        t.Id,
                        t.Name,
                        t.Amount,
                        t.OperationDate,
                        t.IsMonthly,
                        t.TransactionType.Name
                    )
            )
            .ToList();
    }
}

