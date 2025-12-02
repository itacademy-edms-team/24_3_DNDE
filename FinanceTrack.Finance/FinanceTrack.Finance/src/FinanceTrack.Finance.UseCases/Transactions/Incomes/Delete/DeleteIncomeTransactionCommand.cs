using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.Transactions.Delete;

public sealed record DeleteIncomeTransactionCommand(Guid TransactionId, string UserId)
  : ICommand<Result>;
