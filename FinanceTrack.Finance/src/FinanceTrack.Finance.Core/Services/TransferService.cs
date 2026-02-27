using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.Core.Services;

public class TransferService(
    IRepository<FinancialTransaction> _transactionRepo,
    IRepository<Wallet> _walletRepo
)
{
    public async Task<Result<Guid>> Execute(
        CreateTransferRequest request,
        CancellationToken ct = default
    )
    {
        if (request.FromWalletId == request.ToWalletId)
            return Result.Error("Cannot transfer to the same wallet.");

        var spec = new WalletByIdSpec(request.FromWalletId);
        var fromWallet = await _walletRepo.FirstOrDefaultAsync(spec, ct);
        if (fromWallet is null)
            return Result.NotFound("Source wallet not found.");
        if (!string.Equals(fromWallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();
        if (fromWallet.IsArchived)
            return Result.Error("Cannot transfer from an archived wallet.");

        spec = new WalletByIdSpec(request.ToWalletId);
        var toWallet = await _walletRepo.FirstOrDefaultAsync(spec, ct);
        if (toWallet is null)
            return Result.NotFound("Destination wallet not found.");
        if (!string.Equals(toWallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();
        if (toWallet.IsArchived)
            return Result.Error("Cannot transfer to an archived wallet.");

        // Debit source
        try
        {
            fromWallet.Debit(request.Amount);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Error(ex.Message);
        }

        // Credit destination
        toWallet.Credit(request.Amount);

        // Create and persist TransferOut first (with placeholder for related id)
        var transferOut = FinancialTransaction.CreateTransferOut(
            request.UserId,
            request.FromWalletId,
            request.Name,
            request.Amount,
            request.OperationDate,
            Guid.Empty // placeholder, will update after TransferIn is persisted
        );
        await _transactionRepo.AddAsync(transferOut, ct);

        // Now create TransferIn with TransferOut's real Id (assigned by DB)
        var transferIn = FinancialTransaction.CreateTransferIn(
            request.UserId,
            request.ToWalletId,
            request.Name,
            request.Amount,
            request.OperationDate,
            transferOut.Id
        );
        await _transactionRepo.AddAsync(transferIn, ct);

        // Update TransferOut with TransferIn's real Id
        transferOut.SetRelatedTransactionId(transferIn.Id);
        await _transactionRepo.UpdateAsync(transferOut, ct);

        await _walletRepo.UpdateAsync(fromWallet, ct);
        await _walletRepo.UpdateAsync(toWallet, ct);

        return Result.Success(transferOut.Id);
    }
}
