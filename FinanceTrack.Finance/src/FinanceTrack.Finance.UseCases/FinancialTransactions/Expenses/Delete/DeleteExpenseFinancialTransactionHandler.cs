using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Delete;

public class DeleteExpenseFinancialTransactionHandler(
    IDeleteExpenseFinancialTransactionService _service
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
