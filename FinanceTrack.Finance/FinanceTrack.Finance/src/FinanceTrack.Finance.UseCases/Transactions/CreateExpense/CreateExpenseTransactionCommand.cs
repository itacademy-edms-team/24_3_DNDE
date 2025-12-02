using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Transactions.CreateExpense;

public sealed record CreateExpenseTransactionCommand(
  string UserId,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly,
  Guid IncomeTransactionId
) : ICommand<Result<Guid>>;
