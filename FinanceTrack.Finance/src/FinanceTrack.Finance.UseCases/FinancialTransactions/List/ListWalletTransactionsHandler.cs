using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Core.WalletAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public sealed class ListWalletTransactionsHandler(
    IReadRepository<FinancialTransaction> transactionRepo,
    IReadRepository<Wallet> walletRepo
) : IQueryHandler<ListWalletTransactionsQuery, Result<IReadOnlyList<FinancialTransactionDto>>>
{
    public async Task<Result<IReadOnlyList<FinancialTransactionDto>>> Handle(
        ListWalletTransactionsQuery request,
        CancellationToken ct
    )
    {
        var walletSpec = new WalletByIdSpec(request.WalletId);
        var wallet = await walletRepo.FirstOrDefaultAsync(walletSpec, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        FinancialTransactionType? typeFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (
                !FinancialTransactionType.TryFromName(
                    request.Type,
                    ignoreCase: true,
                    out var parsed
                )
            )
                return Result.Error($"Invalid transaction type: {request.Type}");
            typeFilter = parsed;
        }

        var spec = new WalletTransactionsSpec(
            request.UserId,
            request.WalletId,
            typeFilter,
            request.From,
            request.To
        );

        var transactions = await transactionRepo.ListAsync(spec, ct);

        // Processing related transactions
        var relatedTransactionIds = transactions
            .Where(t => t.RelatedTransactionId.HasValue)
            .Select(t => t.RelatedTransactionId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, FinancialTransaction> relatedTransactions = new();
        if (relatedTransactionIds.Any())
        {
            var relatedSpec = new TransactionsByIdsSpec(relatedTransactionIds);
            var related = await transactionRepo.ListAsync(relatedSpec, ct);
            relatedTransactions = related.ToDictionary(t => t.Id);
        }

        var relatedWalletIds = relatedTransactions
            .Values.Select(t => t.WalletId)
            .Distinct()
            .ToList();

        Dictionary<Guid, Wallet> relatedWallets = new();
        if (relatedWalletIds.Any())
        {
            var wallets = await walletRepo.ListAsync(new WalletsByIdsSpec(relatedWalletIds), ct);
            relatedWallets = wallets.ToDictionary(w => w.Id);
        }
        // End Processing related transactions

        IReadOnlyList<FinancialTransactionDto> dtos = transactions
            .Select(t =>
            {
                Guid? relatedWalletId = null;
                string? relatedWalletName = null;

                if (
                    t.RelatedTransactionId.HasValue
                    && relatedTransactions.TryGetValue(
                        t.RelatedTransactionId.Value,
                        out var relatedTransaction
                    )
                )
                {
                    relatedWalletId = relatedTransaction.WalletId;
                    if (
                        relatedWallets.TryGetValue(
                            relatedTransaction.WalletId,
                            out var relatedWallet
                        )
                    )
                    {
                        relatedWalletName = relatedWallet.Name;
                    }
                }

                return new FinancialTransactionDto(
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
                    relatedWalletId,
                    relatedWalletName
                );
            })
            .ToList();

        return Result.Success(dtos);
    }
}
