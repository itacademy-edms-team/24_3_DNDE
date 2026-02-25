using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Create;

public sealed class CreateRecurringTransactionHandler(
    IRepository<RecurringTransaction> _repo,
    IReadRepository<Wallet> _walletRepo
) : ICommandHandler<CreateRecurringTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateRecurringTransactionCommand request,
        CancellationToken ct
    )
    {
        if (!RecurringTransactionType.TryFromName(request.Type, ignoreCase: true, out var txType))
            return Result.Error($"Invalid recurring transaction type: {request.Type}. Must be 'Income' or 'Expense'.");

        var wallet = await _walletRepo.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        var recurring = RecurringTransaction.Create(
            request.UserId,
            request.WalletId,
            request.Name,
            txType,
            request.Amount,
            request.DayOfMonth,
            request.StartDate,
            request.EndDate,
            request.CategoryId
        );

        await _repo.AddAsync(recurring, ct);
        return Result.Success(recurring.Id);
    }
}
