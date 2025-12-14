using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record UpdateExpenseFinancialTransactionRequest(
    Guid TransactionId,
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    Guid IncomeTransactionId
);
