using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Income;

public sealed class CreateIncomeHandler(CreateIncomeService service, IUnitOfWork unitOfWork)
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

        var result = await service.Execute(coreRequest, ct);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}
