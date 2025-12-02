using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.UseCases.Transactions.Delete;

public sealed class DeleteIncomeTransactionHandler(IRepository<Transaction> repository)
  : ICommandHandler<DeleteIncomeTransactionCommand, Result>
{
  public async Task<Result> Handle(
    DeleteIncomeTransactionCommand request,
    CancellationToken cancellationToken
  )
  {
    var transaction = await repository.GetByIdAsync(request.TransactionId, cancellationToken);

    if (transaction is null)
      return Result.NotFound();

    if (!string.Equals(transaction.UserId, request.UserId, StringComparison.Ordinal))
      return Result.Forbidden();

    if (transaction.TransactionType != TransactionType.Income)
      return Result.Error("The specified transaction is not an income transaction.");

    await repository.DeleteAsync(transaction, cancellationToken);

    return Result.Success();
  }
}
