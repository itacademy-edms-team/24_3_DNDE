using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.List;

public interface IListUserIncomeTransactionsQueryService
{
  Task<Result<IReadOnlyList<TransactionDto>>> GetUserIncomeTransactions(
    string userId,
    CancellationToken cancellationToken = default
  );
}
