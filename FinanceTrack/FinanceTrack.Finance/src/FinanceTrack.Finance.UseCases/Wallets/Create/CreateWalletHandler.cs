using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.Wallets.Create;

public sealed class CreateWalletHandler(CreateWalletService service, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateWalletCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateWalletCommand request, CancellationToken ct)
    {
        var result = await service.Execute(request, ct);

        if (result.IsSuccess)
            await unitOfWork.SaveChangesAsync(ct);

        return result;
    }
}
