using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class DeleteExpenseFinancialTransactionService(IRepository<FinancialTransaction> _repo)
{
    public async Task<Result> DeleteExpenseFinancialTransaction(
        DeleteExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        var expenseTransaction = await _repo.GetByIdAsync(request.TransactionId, cancellationToken);
        if (expenseTransaction == null)
            return Result.NotFound();

        if (!expenseTransaction.UserId.Equals(request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (!expenseTransaction.TransactionType.Equals(FinancialTransactionType.Expense))
            return Result.Error("Transaction with transactionId must be Expense type.");

        await _repo.DeleteAsync(expenseTransaction, cancellationToken);

        return Result.Success();
    }
}
