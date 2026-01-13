using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record CreateIncomeFinancialTransactionRequest(
    string userId,
    string name,
    decimal amount,
    DateOnly operationDate,
    bool isMonthly
);
