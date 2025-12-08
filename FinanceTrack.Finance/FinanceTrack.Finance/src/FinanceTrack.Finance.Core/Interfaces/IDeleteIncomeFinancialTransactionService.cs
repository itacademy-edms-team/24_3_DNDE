namespace FinanceTrack.Finance.Core.Interfaces;

public interface IDeleteIncomeFinancialTransactionService
{
    Task<Result> DeleteIncomeTransaction(
        Guid transactionId,
        string userId,
        CancellationToken cancellationToken = default
    );
}
