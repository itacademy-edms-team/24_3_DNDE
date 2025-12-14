using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Create;

public sealed record CreateExpenseFinancialTransactionCommand(
    string UserId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    bool IsMonthly,
    Guid IncomeTransactionId
) : ICommand<Result<Guid>>;
