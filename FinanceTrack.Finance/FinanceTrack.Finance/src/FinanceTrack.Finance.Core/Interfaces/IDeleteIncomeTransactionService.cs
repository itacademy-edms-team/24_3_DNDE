namespace FinanceTrack.Finance.Core.Interfaces;

public interface IDeleteIncomeTransactionService
{
    Task<Result> DeleteIncomeTransaction(
        Guid transactionId,
        string userId,
        CancellationToken cancellationToken = default
    );
}
