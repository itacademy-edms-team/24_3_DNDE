using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.TransactionAggregate;
using MediatR;

namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.Update;

public sealed class UpdateIncomeTransactionHandler(IRepository<Transaction> _repository)
  : ICommandHandler<UpdateIncomeTransactionCommand, Result<TransactionDTO>>
{
  public async Task<Result<TransactionDTO>> Handle(
    UpdateIncomeTransactionCommand request,
    CancellationToken cancellationToken
  )
  {
    var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken);
    if (transaction is null)
      return Result.NotFound();

    if (!string.Equals(transaction.UserId, request.UserId, StringComparison.Ordinal))
      return Result.Forbidden();

    if (transaction.TransactionType != TransactionType.Income)
      return Result.Error("Only income transactions can be updated with this operation.");

    transaction
      .UpdateAmount(request.Amount)
      .SetOperationDate(request.OperationDate)
      .SetMonthly(request.IsMonthly);
    await _repository.UpdateAsync(transaction, cancellationToken);

    var dto = new TransactionDTO(
      transaction.Id,
      transaction.Amount,
      transaction.OperationDate,
      transaction.IsMonthly,
      transaction.TransactionType.Name
    );

    return Result.Success(dto);
  }
}
