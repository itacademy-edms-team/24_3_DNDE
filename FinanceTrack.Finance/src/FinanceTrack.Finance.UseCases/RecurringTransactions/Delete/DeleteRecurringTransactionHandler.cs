using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Delete;

public sealed class DeleteRecurringTransactionHandler(
    IRepository<RecurringTransaction> _repo,
    IUnitOfWork _unitOfWork
) : ICommandHandler<DeleteRecurringTransactionCommand, Result>
{
    public async Task<Result> Handle(DeleteRecurringTransactionCommand request, CancellationToken ct)
    {
        var recurring = await _repo.GetByIdAsync(request.RecurringId, ct);
        if (recurring is null)
            return Result.NotFound();
        if (!string.Equals(recurring.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        await _repo.DeleteAsync(recurring, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
