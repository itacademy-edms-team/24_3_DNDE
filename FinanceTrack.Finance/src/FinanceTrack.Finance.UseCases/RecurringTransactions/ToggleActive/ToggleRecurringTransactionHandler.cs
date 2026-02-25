using FinanceTrack.Finance.Core.RecurringTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.ToggleActive;

public sealed class ToggleRecurringTransactionHandler(IRepository<RecurringTransaction> _repo)
    : ICommandHandler<ToggleRecurringTransactionCommand, Result>
{
    public async Task<Result> Handle(
        ToggleRecurringTransactionCommand request,
        CancellationToken ct
    )
    {
        var recurring = await _repo.GetByIdAsync(request.RecurringId, ct);
        if (recurring is null)
            return Result.NotFound();
        if (!string.Equals(recurring.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (request.IsActive)
            recurring.Activate();
        else
            recurring.Deactivate();

        await _repo.UpdateAsync(recurring, ct);
        return Result.Success();
    }
}
