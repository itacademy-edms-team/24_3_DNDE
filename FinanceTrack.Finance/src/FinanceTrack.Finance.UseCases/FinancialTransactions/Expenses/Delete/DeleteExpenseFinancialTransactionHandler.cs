using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Delete;

public class DeleteExpenseFinancialTransactionHandler(
    DeleteExpenseFinancialTransactionService _service
) : ICommandHandler<DeleteExpenseFinancialTransactionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteExpenseFinancialTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var coreRequest = new DeleteExpenseFinancialTransactionRequest(
            TransactionId: request.TransactionId,
            UserId: request.UserId
        );

        return await _service.DeleteExpenseFinancialTransaction(coreRequest, cancellationToken);
    }
}
