using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Income;

public sealed class CreateIncomeHandler(CreateIncomeService service, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateIncomeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIncomeCommand request, CancellationToken ct)
    {
        var result = await service.Execute(request, ct);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}
