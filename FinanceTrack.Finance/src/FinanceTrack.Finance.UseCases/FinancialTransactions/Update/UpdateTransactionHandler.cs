using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Update;

public sealed class UpdateTransactionHandler(
    UpdateTransactionService service,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateTransactionCommand, Result<FinancialTransactionDto>>
{
    public async Task<Result<FinancialTransactionDto>> Handle(
        UpdateTransactionCommand request,
        CancellationToken ct
    )
    {
        var coreRequest = new UpdateTransactionRequest(
            TransactionId: request.TransactionId,
            UserId: request.UserId,
            Name: request.Name,
            Description: request.Description,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            CategoryId: request.CategoryId
        );

        var result = await service.Execute(coreRequest, ct);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return result.Map(t => new FinancialTransactionDto(
            t.Id,
            t.WalletId,
            t.Name,
            t.Description,
            t.Amount,
            t.OperationDate,
            t.TransactionType.Name,
            t.CategoryId,
            t.RelatedTransactionId,
            t.RecurringTransactionId,
            null, // RelatedWalletId - not loaded during update
            null // RelatedWalletName - not loaded during update
        ));
    }
}
