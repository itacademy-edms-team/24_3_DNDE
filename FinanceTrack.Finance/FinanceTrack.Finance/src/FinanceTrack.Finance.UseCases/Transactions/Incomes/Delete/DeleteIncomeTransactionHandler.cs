using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Transactions.Delete;

public sealed class DeleteIncomeTransactionHandler(IDeleteIncomeTransactionService _service)
    : ICommandHandler<DeleteIncomeTransactionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteIncomeTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _service.DeleteIncomeTransaction(
            request.TransactionId,
            request.UserId,
            cancellationToken
        );
    }
}
