using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Transfer;

public sealed class CreateTransferHandler(TransferService service, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTransferCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTransferCommand request, CancellationToken ct)
    {
        var result = await service.Execute(request, ct);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}
