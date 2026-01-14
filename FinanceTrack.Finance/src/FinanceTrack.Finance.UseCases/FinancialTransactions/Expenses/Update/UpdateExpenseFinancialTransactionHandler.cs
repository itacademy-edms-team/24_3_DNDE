using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Update;

public sealed class UpdateExpenseFinancialTransactionHandler(
    UpdateExpenseFinancialTransactionService _service
) : ICommandHandler<UpdateExpenseFinancialTransactionCommand, Result<FinancialTransactionDto>>
{
    public async Task<Result<FinancialTransactionDto>> Handle(
        UpdateExpenseFinancialTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var coreRequest = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: request.TransactionId,
            UserId: request.UserId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            IsMonthly: request.IsMonthly,
            IncomeTransactionId: request.IncomeTransactionId
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            coreRequest,
            cancellationToken
        );

        return result.Map(transaction => new FinancialTransactionDto(
            transaction.Id,
            transaction.Name,
            transaction.Amount,
            transaction.OperationDate,
            transaction.IsMonthly,
            transaction.TransactionType.Name
        ));
    }
}
