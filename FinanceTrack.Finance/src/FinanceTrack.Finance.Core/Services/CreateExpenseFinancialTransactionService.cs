using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class CreateExpenseFinancialTransactionService(IRepository<FinancialTransaction> _repo)
    : ICreateExpenseFinancialTransactionService
{
    public async Task<Result<Guid>> CreateExpenseFinancialTransaction(
        CreateExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        var incomeTransaction = await _repo.GetByIdAsync(
            request.IncomeTransactionId,
            cancellationToken
        );
        if (incomeTransaction == null)
            return Result.NotFound();

        if (!incomeTransaction.UserId.Equals(request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (!incomeTransaction.TransactionType.Equals(FinancialTransactionType.Income))
            return Result.Error("Transaction with incomeTransactionId must be Income type.");

        if (incomeTransaction.OperationDate > request.OperationDate)
            return Result.Error(
                "Expense operation date must be greater or equal than income operation date."
            );

        var expenseTransaction = FinancialTransaction.CreateExpense(
            request.UserId,
            request.Name,
            request.Amount,
            request.OperationDate,
            request.IsMonthly,
            request.IncomeTransactionId
        );
        await _repo.AddAsync(expenseTransaction, cancellationToken);

        return Result.Success(expenseTransaction.Id);
    }
}
