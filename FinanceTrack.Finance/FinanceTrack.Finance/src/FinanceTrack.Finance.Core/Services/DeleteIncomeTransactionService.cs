using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.Core.Services;

public class DeleteIncomeTransactionService(
    IRepository<Transaction> _repository,
    ILogger<DeleteIncomeTransactionService> _logger
) : IDeleteIncomeTransactionService
{
    public async Task<Result> DeleteIncomeTransaction(
        Guid transactionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Deleting income transaction {TransactionId} for user {UserId}",
            transactionId,
            userId
        );

        var transaction = await _repository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
            return Result.NotFound();
        if (!string.Equals(transaction.UserId, userId, StringComparison.Ordinal))
            return Result.Forbidden();
        if (transaction.TransactionType != TransactionType.Income)
            return Result.Error("The specified transaction is not an income transaction.");

        await _repository.DeleteAsync(transaction, cancellationToken);
        return Result.Success();
    }
}
