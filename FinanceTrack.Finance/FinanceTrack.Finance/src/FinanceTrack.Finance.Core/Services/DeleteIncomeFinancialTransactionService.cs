using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class DeleteIncomeFinancialTransactionService(
    IRepository<FinancialTransaction> _repository,
    ILogger<DeleteIncomeFinancialTransactionService> _logger
) : IDeleteIncomeFinancialTransactionService
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
        if (transaction.TransactionType != FinancialTransactionType.Income)
            return Result.Error("The specified transaction is not an income transaction.");

        await _repository.DeleteAsync(transaction, cancellationToken);
        return Result.Success();
    }
}
