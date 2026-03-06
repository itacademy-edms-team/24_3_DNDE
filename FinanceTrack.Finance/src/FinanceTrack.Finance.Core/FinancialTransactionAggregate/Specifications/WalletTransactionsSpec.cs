namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class WalletTransactionsSpec : Specification<FinancialTransaction>
{
    public WalletTransactionsSpec(
        string userId,
        Guid walletId,
        FinancialTransactionType? type = null,
        DateOnly? from = null,
        DateOnly? to = null
    )
    {
        Query
            .Where(t => t.UserId == userId && t.WalletId == walletId)
            .Where(t => type == null || t.TransactionType == type)
            .Where(t => from == null || t.OperationDate >= from)
            .Where(t => to == null || t.OperationDate <= to)
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAtUtc);
    }
}
