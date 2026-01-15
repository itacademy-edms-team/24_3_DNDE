using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class UpdateExpenseFinancialTransactionService(IRepository<FinancialTransaction> _repo)
{
    public async Task<Result<FinancialTransaction>> UpdateExpenseFinancialTransaction(
        UpdateExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var income = await _repo.GetByIdAsync(request.IncomeTransactionId, cancellationToken);

        if (income == null)
            return Result.NotFound();

        if (!income.UserId.Equals(request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (!income.TransactionType.Equals(FinancialTransactionType.Income))
            return Result.Error("Transaction with incomeTransactionId must be Income type.");

        if (income.OperationDate > request.OperationDate)
            return Result.Error(
                "Expense operation date must be greater or equal than income operation date."
            );

        if (!income.IsMonthly && request.IsMonthly)
        {
            return Result.Error(
                "Cannot set expense to monthly when parent income is non-monthly. Expense must be non-monthly when income is non-monthly."
            );
        }

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
