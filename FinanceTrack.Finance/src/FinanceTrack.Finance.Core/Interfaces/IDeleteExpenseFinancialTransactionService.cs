namespace FinanceTrack.Finance.Core.Interfaces;

public interface IDeleteExpenseFinancialTransactionService
{
    Task<Result> DeleteExpenseFinancialTransaction(
        DeleteExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    );
}
