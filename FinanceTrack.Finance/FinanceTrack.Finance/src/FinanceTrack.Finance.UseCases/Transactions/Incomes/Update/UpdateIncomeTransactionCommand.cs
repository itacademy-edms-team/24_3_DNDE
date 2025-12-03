using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Transactions.Incomes.Update;

public sealed record UpdateIncomeTransactionCommand(
  Guid TransactionId,
  string UserId,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly
) : ICommand<Result<TransactionDTO>>;
