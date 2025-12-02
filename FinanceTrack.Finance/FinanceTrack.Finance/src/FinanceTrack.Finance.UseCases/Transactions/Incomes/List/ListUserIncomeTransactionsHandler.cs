using FinanceTrack.Finance.Core.TransactionAggregate;
using FinanceTrack.Finance.Core.TransactionAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.List;

public sealed class ListUserIncomeTransactionsHandler(IReadRepository<Transaction> repository)
  : IQueryHandler<ListUserIncomeTransactionsQuery, IReadOnlyList<TransactionDTO>>
{
  public async Task<IReadOnlyList<TransactionDTO>> Handle(
    ListUserIncomeTransactionsQuery request,
    CancellationToken cancellationToken
  )
  {
    var spec = new UserIncomeTransactionsSpec(request.UserId);

    var items = await repository.ListAsync(spec, cancellationToken);

    return items
      .Select(t => new TransactionDTO(
        t.Id,
        t.Amount,
        t.OperationDate,
        t.IsMonthly,
        t.TransactionType.Name
      ))
      .ToList();
  }
}
