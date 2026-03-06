using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.RecurringTransactions.Update;

public sealed class UpdateRecurringTransactionHandler(
    IRepository<RecurringTransaction> repo,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateRecurringTransactionCommand, Result<RecurringTransactionDto>>
{
    public async Task<Result<RecurringTransactionDto>> Handle(
        UpdateRecurringTransactionCommand request,
        CancellationToken ct
    )
    {
        var spec = new RecurringTransactionByIdSpec(request.RecurringId);
        var recurring = await repo.FirstOrDefaultAsync(spec, ct);
        if (recurring is null)
            return Result.NotFound();
        if (!string.Equals(recurring.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        recurring
            .UpdateName(request.Name)
            .UpdateAmount(request.Amount)
            .SetDayOfMonth(request.DayOfMonth)
            .SetEndDate(request.EndDate)
            .SetCategory(request.CategoryId);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(
            new RecurringTransactionDto(
                recurring.Id,
                recurring.WalletId,
                recurring.CategoryId,
                recurring.Name,
                recurring.TransactionType.Name,
                recurring.Amount,
                recurring.DayOfMonth,
                recurring.StartDate,
                recurring.EndDate,
                recurring.IsActive,
                recurring.LastProcessedDate
            )
        );
    }
}
