using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.UseCases.Transactions;

public sealed record TransactionDTO(
  Guid Id,
  decimal Amount,
  DateOnly OperationDate,
  bool IsMonthly,
  string Type
);
