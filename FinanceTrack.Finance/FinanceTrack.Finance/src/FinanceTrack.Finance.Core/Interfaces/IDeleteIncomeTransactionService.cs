using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.Interfaces;

public interface IDeleteIncomeTransactionService
{
  Task<Result> DeleteIncomeTransaction(
    Guid transactionId,
    string userId,
    CancellationToken cancellationToken = default
  );
}
