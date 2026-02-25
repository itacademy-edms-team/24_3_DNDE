using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Income;

public sealed class CreateIncomeHandler(CreateIncomeService _service)
    : ICommandHandler<CreateIncomeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIncomeCommand request, CancellationToken ct)
    {
        var coreRequest = new CreateIncomeRequest(
            UserId: request.UserId,
            WalletId: request.WalletId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            CategoryId: request.CategoryId
        );

        return await _service.Execute(coreRequest, ct);
    }
}
