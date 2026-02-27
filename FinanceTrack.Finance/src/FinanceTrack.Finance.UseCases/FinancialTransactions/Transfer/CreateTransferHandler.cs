using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Transfer;

public sealed class CreateTransferHandler(
    TransferService _service,
    IUnitOfWork _unitOfWork
) : ICommandHandler<CreateTransferCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTransferCommand request, CancellationToken ct)
    {
        var coreRequest = new CreateTransferRequest(
            UserId: request.UserId,
            FromWalletId: request.FromWalletId,
            ToWalletId: request.ToWalletId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate
        );

        var result = await _service.Execute(coreRequest, ct);

        if (result.IsSuccess)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}
