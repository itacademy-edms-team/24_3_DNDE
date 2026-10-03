using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.Core.Services;

public class DeleteTransactionService(IRepository<FinancialTransaction> transactionRepo)
{
    public async Task<Result> Execute(
        Guid transactionId,
        string userId,
        CancellationToken ct = default
    )
    {
        var transactionSpec = new FinancialTransactionByIdSpec(transactionId);
        var transaction = await transactionRepo.FirstOrDefaultAsync(transactionSpec, ct);
        if (transaction is null)
            return Result.NotFound();
        if (!string.Equals(transaction.UserId, userId, StringComparison.Ordinal))
            return Result.Forbidden();

        var wallet = transaction.Wallet;
        if (wallet is null)
            return Result.NotFound("Wallet not found.");

        // Reverse the balance impact of the main transaction
        var mainReverseError = ReverseWalletImpact(wallet, transaction);
        if (mainReverseError is not null)
            return Result.Error(mainReverseError);

        // If it's a transfer, also delete the related transaction and reverse its wallet
        if (transaction.RelatedTransactionId.HasValue)
        {
            var relatedResult = await ReverseAndDeleteRelatedTransactionAsync(transaction, ct);
            if (!relatedResult.IsSuccess)
                return relatedResult;
        }

        await transactionRepo.DeleteAsync(transaction, ct);

        return Result.Success();
    }

    /// <summary>
    /// Reverses the impact of a transaction on the given wallet.
    /// Returns error message if reversal is not possible, otherwise null.
    /// </summary>
    private static string? ReverseWalletImpact(Wallet wallet, FinancialTransaction transaction)
    {
        if (
            transaction.TransactionType == FinancialTransactionType.Income
            || transaction.TransactionType == FinancialTransactionType.TransferIn
        )
        {
            // Was a credit, so debit to reverse.
            // Catch errors related to negative balance restrictions. Example: delete transaction -> balance still negative.
            try
            {
                wallet.Debit(transaction.Amount);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
        else if (
            transaction.TransactionType == FinancialTransactionType.Expense
            || transaction.TransactionType == FinancialTransactionType.TransferOut
        )
        {
            // Was a debit, so credit to reverse.
            wallet.Credit(transaction.Amount);
        }

        return null;
    }

    /// <summary>
    /// For transfer transactions, reverses and deletes the related paired transaction.
    /// </summary>
    private async Task<Result> ReverseAndDeleteRelatedTransactionAsync(
        FinancialTransaction transaction,
        CancellationToken ct
    )
    {
        var relatedSpec = new TransactionByRelatedIdSpec(transaction.Id);
        var related = await transactionRepo.FirstOrDefaultAsync(relatedSpec, ct);

        if (related is null)
            return Result.Success();

        var relatedWallet = related.Wallet;
        if (relatedWallet is not null)
        {
            var reverseError = ReverseWalletImpact(relatedWallet, related);
            if (reverseError is not null)
                return Result.Error(reverseError);
        }

        await transactionRepo.DeleteAsync(related, ct);

        return Result.Success();
    }
}
