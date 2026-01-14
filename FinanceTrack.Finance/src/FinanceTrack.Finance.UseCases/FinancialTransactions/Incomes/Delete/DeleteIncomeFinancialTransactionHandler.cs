using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;

public sealed class DeleteIncomeFinancialTransactionHandler(
    DeleteIncomeFinancialTransactionService _service
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
