using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Delete;

public sealed record DeleteExpenseFinancialTransactionCommand(Guid TransactionId, string UserId)
    : ICommand<Result>;
