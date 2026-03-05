using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.Core.Services;

public class CreateExpenseService(
    IRepository<FinancialTransaction> transactionRepo,
    IRepository<Wallet> walletRepo
)
{
    public async Task<Result<Guid>> Execute(
        CreateExpenseRequest request,
        CancellationToken ct = default
    )
    {
        var spec = new WalletByIdSpec(request.WalletId);
        var wallet = await walletRepo.FirstOrDefaultAsync(spec, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();
        if (wallet.IsArchived)
            return Result.Error("Cannot add expense to an archived wallet.");

        var transaction = FinancialTransaction.CreateExpense(
            request.UserId,
            request.WalletId,
            request.Name,
            request.Amount,
            request.OperationDate,
            request.CategoryId,
            request.RecurringTransactionId
        );

        // Debit will throw if insufficient funds and AllowNegativeBalance is false
        try
        {
            wallet.Debit(request.Amount);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Error(ex.Message);
        }

        wallet.AddTransaction(transaction);

        await transactionRepo.AddAsync(transaction, ct);

        return Result.Success(transaction.Id);
    }
}
