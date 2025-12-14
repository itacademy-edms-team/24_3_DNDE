using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Update;

public sealed class UpdateExpenseFinancialTransactionHandler(
    IUpdateExpenseFinancialTransactionService _service
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

        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => Result.NotFound(),
                ResultStatus.Forbidden => Result.Forbidden(),
                _ => Result.Error(new ErrorList(result.Errors)),
            };
        }

        var transaction = result.Value;
        var dto = new FinancialTransactionDto(
            transaction.Id,
            transaction.Name,
            transaction.Amount,
            transaction.OperationDate,
            transaction.IsMonthly,
            transaction.TransactionType.Name
        );

        return Result.Success(dto);
    }
}
