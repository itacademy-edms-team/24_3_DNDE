using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.Services;

public class DeleteTransactionService(
    IRepository<FinancialTransaction> _transactionRepo,
    IRepository<Wallet> _walletRepo
)
{
    public async Task<Result> Execute(
        Guid transactionId,
        string userId,
        CancellationToken ct = default
    )
    {
        var transaction = await _transactionRepo.GetByIdAsync(transactionId, ct);
        if (transaction is null)
            return Result.NotFound();
        if (!string.Equals(transaction.UserId, userId, StringComparison.Ordinal))
            return Result.Forbidden();

        var wallet = await _walletRepo.GetByIdAsync(transaction.WalletId, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");

        // Reverse the balance impact
        if (
            transaction.TransactionType == FinancialTransactionType.Income
            || transaction.TransactionType == FinancialTransactionType.TransferIn
        )
        {
            // Was a credit, so debit to reverse. Force-allow negative since we're reverting.
            wallet.Debit(transaction.Amount);
        }
        else if (
            transaction.TransactionType == FinancialTransactionType.Expense
            || transaction.TransactionType == FinancialTransactionType.TransferOut
        )
        {
            // Was a debit, so credit to reverse
            wallet.Credit(transaction.Amount);
        }

        // If it's a transfer, also delete the related transaction and reverse its wallet
        if (transaction.RelatedTransactionId.HasValue)
        {
            var relatedSpec = new TransactionByRelatedIdSpec(transaction.Id);
            var related = await _transactionRepo.FirstOrDefaultAsync(relatedSpec, ct);

            if (related is not null)
            {
                var relatedWallet = await _walletRepo.GetByIdAsync(related.WalletId, ct);
                if (relatedWallet is not null)
                {
                    if (
                        related.TransactionType == FinancialTransactionType.Income
                        || related.TransactionType == FinancialTransactionType.TransferIn
                    )
                    {
                        relatedWallet.Debit(related.Amount);
                    }
                    else
                    {
                        relatedWallet.Credit(related.Amount);
                    }

                    await _walletRepo.UpdateAsync(relatedWallet, ct);
                }

                await _transactionRepo.DeleteAsync(related, ct);
            }
        }

        await _transactionRepo.DeleteAsync(transaction, ct);
        await _walletRepo.UpdateAsync(wallet, ct);

        return Result.Success();
    }
}
