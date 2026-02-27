using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;

public sealed class DeleteTransactionHandler(
    DeleteTransactionService _service,
    IUnitOfWork _unitOfWork
) : ICommandHandler<DeleteTransactionCommand, Result>
{
    public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken ct)
    {
        var result = await _service.Execute(request.TransactionId, request.UserId, ct);

        if (result.IsSuccess)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}
