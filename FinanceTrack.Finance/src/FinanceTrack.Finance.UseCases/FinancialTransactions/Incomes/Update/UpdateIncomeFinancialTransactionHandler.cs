using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Update;

public sealed class UpdateIncomeFinancialTransactionHandler(
    IUpdateIncomeFinancialTransactionService _service
) : ICommandHandler<UpdateIncomeFinancialTransactionCommand, Result<FinancialTransactionDto>>
{
    public async Task<Result<FinancialTransactionDto>> Handle(
        UpdateIncomeFinancialTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var coreRequest = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: request.TransactionId,
            UserId: request.UserId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            IsMonthly: request.IsMonthly
        );

        var result = await _service.UpdateIncome(coreRequest, cancellationToken);

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
