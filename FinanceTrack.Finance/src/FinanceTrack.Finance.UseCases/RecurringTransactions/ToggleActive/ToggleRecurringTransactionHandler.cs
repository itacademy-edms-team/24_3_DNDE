using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.ToggleActive;

public sealed class ToggleRecurringTransactionHandler(
    IRepository<RecurringTransaction> _repo,
    IUnitOfWork _unitOfWork
) : ICommandHandler<ToggleRecurringTransactionCommand, Result>
{
    public async Task<Result> Handle(
        ToggleRecurringTransactionCommand request,
        CancellationToken ct
    )
    {
        var spec = new RecurringTransactionByIdSpec(request.RecurringId);
        var recurring = await _repo.FirstOrDefaultAsync(spec, ct);
        if (recurring is null)
            return Result.NotFound();
        if (!string.Equals(recurring.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (request.IsActive)
            recurring.Activate();
        else
            recurring.Deactivate();

        await _repo.UpdateAsync(recurring, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
