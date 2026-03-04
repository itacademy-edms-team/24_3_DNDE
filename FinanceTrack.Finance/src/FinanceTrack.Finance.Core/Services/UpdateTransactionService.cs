using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.Core.Services;

public class UpdateTransactionService(
    IRepository<FinancialTransaction> _transactionRepo,
    IRepository<Wallet> _walletRepo
)
{
    public async Task<Result<FinancialTransaction>> Execute(
        UpdateTransactionRequest request,
        CancellationToken ct = default
    )
    {
        var transactionSpec = new FinancialTransactionByIdSpec(request.TransactionId);
        var transaction = await _transactionRepo.FirstOrDefaultAsync(transactionSpec, ct);
        if (transaction is null)
            return Result.NotFound();
        if (!string.Equals(transaction.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        // Transfer transactions can only update name and date, not amount
        if (
            transaction.TransactionType == FinancialTransactionType.TransferIn
            || transaction.TransactionType == FinancialTransactionType.TransferOut
        )
        {
            return Result.Error(
                "Transfer transactions cannot be edited. Delete and recreate instead."
            );
        }

        var walletSpec = new WalletByIdSpec(transaction.WalletId);
        var wallet = await _walletRepo.FirstOrDefaultAsync(walletSpec, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");

        // Adjust balance for amount difference
        var newAmount = decimal.Round(request.Amount, 2);
        var balanceError = AdjustWalletForAmountChange(wallet, transaction, newAmount);
        if (balanceError is not null)
            return Result.Error(balanceError);

        transaction
            .UpdateName(request.Name)
            .UpdateAmount(request.Amount)
            .SetOperationDate(request.OperationDate)
            .SetCategory(request.CategoryId);

        return Result.Success(transaction);
    }

    /// <summary>
    /// Adjusts wallet balance according to the difference between the existing transaction amount
    /// and the new amount. Returns error message if adjustment is not possible, otherwise null.
    /// </summary>
    private static string? AdjustWalletForAmountChange(
        Wallet wallet,
        FinancialTransaction transaction,
        decimal newAmount
    )
    {
        var oldAmount = transaction.Amount;
        if (oldAmount == newAmount)
            return null;

        var diff = newAmount - oldAmount;

        if (transaction.TransactionType == FinancialTransactionType.Income)
        {
            // Income increased -> credit more, decreased -> debit difference
            if (diff > 0)
            {
                wallet.Credit(diff);
            }
            else
            {
                try
                {
                    wallet.Debit(-diff);
                }
                catch (InvalidOperationException ex)
                {
                    return ex.Message;
                }
            }
        }
        else if (transaction.TransactionType == FinancialTransactionType.Expense)
        {
            // Expense increased -> debit more, decreased -> credit difference
            if (diff > 0)
            {
                try
                {
                    wallet.Debit(diff);
                }
                catch (InvalidOperationException ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                wallet.Credit(-diff);
            }
        }

        return null;
    }
}
