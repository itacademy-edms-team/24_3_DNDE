using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expense;

public sealed class CreateExpenseHandler(
    CreateExpenseService _service,
    IUnitOfWork _unitOfWork
) : ICommandHandler<CreateExpenseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        var coreRequest = new CreateExpenseRequest(
            UserId: request.UserId,
            WalletId: request.WalletId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            CategoryId: request.CategoryId
        );

        var result = await _service.Execute(coreRequest, ct);

        if (result.IsSuccess)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}
