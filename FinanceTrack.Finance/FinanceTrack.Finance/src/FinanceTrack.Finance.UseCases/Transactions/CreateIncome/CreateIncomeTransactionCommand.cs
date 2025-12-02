using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Transactions.CreateIncome;

public sealed record CreateIncomeTransactionCommand(
  string UserId,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly
) : ICommand<Result<Guid>>;
