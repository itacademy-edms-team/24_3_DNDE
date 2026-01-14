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
