using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Create;

public sealed class CreateRecurringTransactionHandler(
    IRepository<RecurringTransaction> recurringTransactionRepo,
    IReadRepository<Wallet> walletRepo,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateRecurringTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateRecurringTransactionCommand request,
        CancellationToken ct
    )
    {
        if (!RecurringTransactionType.TryFromName(request.Type, ignoreCase: true, out var txType))
            return Result.Error(
                $"Invalid recurring transaction type: {request.Type}. Must be 'Income' or 'Expense'."
            );

        var walletSpec = new WalletByIdSpec(request.WalletId);
        var wallet = await walletRepo.FirstOrDefaultAsync(walletSpec, ct);
        if (wallet is null)
            return Result.NotFound("Wallet not found.");
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        var recurring = RecurringTransaction.Create(
            request.UserId,
            request.WalletId,
            request.Name,
            request.Description,
            txType,
            request.Amount,
            request.DayOfMonth,
            request.StartDate,
            request.EndDate,
            request.CategoryId
        );

        await recurringTransactionRepo.AddAsync(recurring, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(recurring.Id);
    }
}
