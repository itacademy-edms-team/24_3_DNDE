using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;

public sealed class DeleteTransactionHandler(DeleteTransactionService _service)
    : ICommandHandler<DeleteTransactionCommand, Result>
{
    public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken ct)
    {
        return await _service.Execute(request.TransactionId, request.UserId, ct);
    }
}
