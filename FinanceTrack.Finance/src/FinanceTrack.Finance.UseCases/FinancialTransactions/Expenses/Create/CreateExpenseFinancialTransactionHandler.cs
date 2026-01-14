using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Create;

public sealed class CreateExpenseFinancialTransactionHandler(
    CreateExpenseFinancialTransactionService _service
) : ICommandHandler<CreateExpenseFinancialTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateExpenseFinancialTransactionCommand request,
        CancellationToken ct
    )
    {
        var coreRequest = new CreateExpenseFinancialTransactionRequest(
            UserId: request.UserId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            IsMonthly: request.IsMonthly,
            IncomeTransactionId: request.IncomeTransactionId
        );

        return await _service.CreateExpenseFinancialTransaction(coreRequest, ct);
    }
}
