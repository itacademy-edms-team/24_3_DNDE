using Ardalis.Result;
using FinanceTrack.Finance.Core.TransactionAggregate;
using FinanceTrack.Finance.Core.TransactionAggregate.Specifications;
using FinanceTrack.Finance.UseCases.Transactions;
using FinanceTrack.Finance.UseCases.Transactions.Incomes.List;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class ListUserIncomeTransactionsQueryService(IRepository<Transaction> _repository)
  : IListUserIncomeTransactionsQueryService
{
  public async Task<Result<IReadOnlyList<TransactionDto>>> GetUserIncomeTransactions(
    string userId,
    CancellationToken cancellationToken = default
  )
  {
    var spec = new UserIncomeTransactionsSpec(userId);

    var items = await _repository.ListAsync(spec, cancellationToken);

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
