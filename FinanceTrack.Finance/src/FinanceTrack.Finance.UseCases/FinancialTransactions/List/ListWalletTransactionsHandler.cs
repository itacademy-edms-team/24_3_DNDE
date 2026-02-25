using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public sealed class ListWalletTransactionsHandler(
    IReadRepository<FinancialTransaction> _transactionRepo,
    IReadRepository<Wallet> _walletRepo
) : IQueryHandler<ListWalletTransactionsQuery, Result<IReadOnlyList<FinancialTransactionDto>>>
{
    public async Task<Result<IReadOnlyList<FinancialTransactionDto>>> Handle(
        ListWalletTransactionsQuery request,
        CancellationToken ct
    )
    {
        var wallet = await _walletRepo.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result.NotFound();
        if (!string.Equals(wallet.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        FinancialTransactionType? typeFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (!FinancialTransactionType.TryFromName(request.Type, ignoreCase: true, out var parsed))
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

        var transactions = await _transactionRepo.ListAsync(spec, ct);

        IReadOnlyList<FinancialTransactionDto> dtos = transactions
            .Select(t => new FinancialTransactionDto(
                t.Id,
                t.WalletId,
                t.Name,
                t.Amount,
                t.OperationDate,
                t.TransactionType.Name,
                t.CategoryId,
                t.RelatedTransactionId,
                t.RecurringTransactionId
            ))
            .ToList();

        return Result.Success(dtos);
    }
}
