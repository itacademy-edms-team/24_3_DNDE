using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.Services;

public class CreateIncomeService(
    IRepository<FinancialTransaction> _transactionRepo,
    IRepository<Wallet> _walletRepo
)
{
    public async Task<Result<Guid>> Execute(
        CreateIncomeRequest request,
        CancellationToken ct = default
    )
    {
        var wallet = await _walletRepo.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();
        if (wallet.IsArchived)
            return Result.Error("Cannot add income to an archived wallet.");

        var transaction = FinancialTransaction.CreateIncome(
            request.UserId,
            request.WalletId,
            request.Name,
            request.Amount,
            request.OperationDate,
            request.CategoryId,
            request.RecurringTransactionId
        );

        wallet.Credit(request.Amount);

        await _transactionRepo.AddAsync(transaction, ct);
        await _walletRepo.UpdateAsync(wallet, ct);

        return Result.Success(transaction.Id);
    }
}
