using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.UseCases.Transactions.Delete;

public sealed class DeleteIncomeTransactionHandler(IDeleteIncomeTransactionService _service)
  : ICommandHandler<DeleteIncomeTransactionCommand, Result>
{
  public async Task<Result> Handle(
    DeleteIncomeTransactionCommand request,
    CancellationToken cancellationToken
  )
  {
    return await _service.DeleteIncomeTransaction(
      request.TransactionId,
      request.UserId,
      cancellationToken
    );
  }
}
