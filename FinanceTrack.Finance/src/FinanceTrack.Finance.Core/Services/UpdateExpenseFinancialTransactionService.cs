using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class UpdateExpenseFinancialTransactionService(IRepository<FinancialTransaction> _repo)
    : IUpdateExpenseFinancialTransactionService
{
    public async Task<Result<FinancialTransaction>> UpdateExpenseFinancialTransaction(
        UpdateExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var expense = await _repo.GetByIdAsync(request.TransactionId, cancellationToken);
        if (expense is null)
            return Result.NotFound();
        if (!string.Equals(expense.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();
        if (expense.TransactionType != FinancialTransactionType.Expense)
            return Result.Error("The specified transaction is not an expense.");

        // Changing parent income is not allowed.
        if (expense.IncomeTransactionId != request.IncomeTransactionId)
            return Result.Error("Changing parent income is not supported.");

        expense
            .UpdateName(request.Name)
            .UpdateAmount(request.Amount)
            .SetOperationDate(request.OperationDate)
            .SetMonthly(request.IsMonthly);

        await _repo.UpdateAsync(expense, cancellationToken);

        return Result.Success(expense);
    }
}
