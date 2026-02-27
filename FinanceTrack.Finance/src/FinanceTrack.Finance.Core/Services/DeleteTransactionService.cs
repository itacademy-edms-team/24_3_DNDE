using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

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
        var transactionSpec = new FinancialTransactionByIdSpec(transactionId);
        var transaction = await _transactionRepo.FirstOrDefaultAsync(transactionSpec, ct);
        if (transaction is null)
            return Result.NotFound();
        if (!string.Equals(transaction.UserId, userId, StringComparison.Ordinal))
            return Result.Forbidden();

        var walletSpec = new WalletByIdSpec(transaction.WalletId);
        var wallet = await _walletRepo.FirstOrDefaultAsync(walletSpec, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");

        // Reverse the balance impact
        if (
            transaction.TransactionType == FinancialTransactionType.Income
            || transaction.TransactionType == FinancialTransactionType.TransferIn
        )
        {
            // Was a credit, so debit to reverse
            try
            {
                wallet.Debit(transaction.Amount);
            }
            // Catch errors related to negative balance restrictions. Example: delete transaction -> balance still negative
            catch (InvalidOperationException ex)
            {
                return Result.Error(ex.Message);
            }
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
                walletSpec = new WalletByIdSpec(related.WalletId);
                var relatedWallet = await _walletRepo.FirstOrDefaultAsync(walletSpec, ct);
                if (relatedWallet is not null)
                {
                    if (
                        related.TransactionType == FinancialTransactionType.Income
                        || related.TransactionType == FinancialTransactionType.TransferIn
                    )
                    {
                        try
                        {
                            relatedWallet.Debit(related.Amount);
                        }
                        catch (InvalidOperationException ex)
                        {
                            return Result.Error(ex.Message);
                        }
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
