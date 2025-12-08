using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;

public sealed class DeleteIncomeFinancialTransactionHandler(
    IDeleteIncomeFinancialTransactionService _service
) : ICommandHandler<DeleteIncomeFinancialTransactionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteIncomeFinancialTransactionCommand request,
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
