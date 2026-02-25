using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;

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
        var transaction = await _transactionRepo.GetByIdAsync(request.TransactionId, ct);
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
            return Result.Error("Transfer transactions cannot be edited. Delete and recreate instead.");
        }

        var wallet = await _walletRepo.GetByIdAsync(transaction.WalletId, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");

        // Adjust balance for amount difference
        var oldAmount = transaction.Amount;
        var newAmount = decimal.Round(request.Amount, 2);

        if (oldAmount != newAmount)
        {
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
                        return Result.Error(ex.Message);
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
                        return Result.Error(ex.Message);
                    }
                }
                else
                {
                    wallet.Credit(-diff);
                }
            }
        }

        transaction
            .UpdateName(request.Name)
            .UpdateAmount(request.Amount)
            .SetOperationDate(request.OperationDate)
            .SetCategory(request.CategoryId);

        await _transactionRepo.UpdateAsync(transaction, ct);
        await _walletRepo.UpdateAsync(wallet, ct);

        return Result.Success(transaction);
    }
}
