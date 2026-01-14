using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Create;

public sealed class CreateIncomeFinancialTransactionHandler(
    CreateIncomeFinancialTransactionService _service
) : ICommandHandler<CreateIncomeFinancialTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateIncomeFinancialTransactionCommand request,
        CancellationToken ct
    )
    {
        var coreRequest = new CreateIncomeFinancialTransactionRequest(
            userId: request.UserId,
            name: request.Name,
            amount: request.Amount,
            operationDate: request.OperationDate,
            isMonthly: request.IsMonthly
        );

        return await _service.AddIncome(coreRequest, ct);
    }
}
